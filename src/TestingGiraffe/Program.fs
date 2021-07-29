open System.Threading
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
open Giraffe
open Giraffe.EndpointRouting
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharp.Azure.Storage.Table
open Microsoft.Azure.Cosmos.Table
open Types

let matchErrorResponse serviceError =
    match serviceError with
    | Domain.Types.ServiceError.UnexpectedError u -> u |> toErrorResponse |> ServerErrors.INTERNAL_ERROR
    | Domain.Types.ServiceError.ValidationError v -> v |> toErrorResponse |> RequestErrors.BAD_REQUEST

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Services.AddGiraffe() |> ignore
    let app = builder.Build()

    if app.Environment.IsDevelopment() then
        app.UseDeveloperExceptionPage() |> ignore

    let cloudClient =
        let connectionString = app.Configuration.["TableStorageConnectionString"] |> TableStorageConnectionString.create
        let account = connectionString |> TableStorageConnectionString.value |> CloudStorageAccount.Parse
        account.CreateCloudTableClient()

    let postGameHandler: HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) -> task {
            let! game = ctx.BindJsonAsync<GameResource>()
            let domainHandler = cloudClient |> TableStorage.Handlers.composeInsertGameHandler |> Domain.Domain.insertNewGame
            let result = game |> toDomainGame |> Result.bind domainHandler
            return!
                (match result with
                | Ok () -> Successful.NO_CONTENT
                | Error ex -> matchErrorResponse ex) next ctx
        }
        
    let getAllGamesHandler: HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) -> task {
            let domainHandler = cloudClient |> TableStorage.Handlers.composeGetAllGamesHandler |> Domain.Domain.getAllGames
            let games = domainHandler()
            return!
                (match games with
                | Ok g -> g |> Seq.map toGameResource |> Successful.OK
                | Error ex -> matchErrorResponse ex) next ctx
        
        }
        
    let getGameByIdHandler (id: string): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) -> task {
            let domainHandler = cloudClient |> TableStorage.Handlers.composeGetGameByIdHandler |> Domain.Domain.getGameById
            let game = domainHandler id
            return!
                (match game with
                | Ok g -> g |> toGameResource |> Successful.OK
                | Error ex -> matchErrorResponse ex) next ctx
        }

    let endpoints = [ 
        GET [
            route   "/" (text "Swagger here some day, ya know")
            route   "/games"    getAllGamesHandler
            routef  "/games/%s" getGameByIdHandler
        ]
        POST [
            route   "/games"    postGameHandler
        ]
    ]

    app.MapGiraffeEndpoints endpoints
    |> ignore

    app.Run()

    0 // Exit code
