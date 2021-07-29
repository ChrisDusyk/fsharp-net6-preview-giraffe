open System.Threading
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
open Giraffe
open Giraffe.EndpointRouting
open FSharp.Control.Tasks.V2.ContextInsensitive

type GameResource = {
    Developer: string
    Id: System.Guid
    Name: string
    HasMultiplayer: bool }

let toGameResource (game: Domain.Types.Game): GameResource =
    { Developer = game.Developer
      Id = game.Id
      Name = game.Name
      HasMultiplayer = game.HasMultiplayer }

let toDomainGame (game: GameResource) =
    let stringGameId = game.Id.ToString()
    Domain.Types.Game.create stringGameId game.Developer game.Name game.HasMultiplayer

let postGameHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let! game = ctx.BindJsonAsync<GameResource>()
        let domainHandler = Domain.Domain.insertNewGame TableStorage.Handlers.insertGameIntoTableStorage
        let result = game |> toDomainGame |> Result.bind domainHandler
        return!
            (match result with
            | Ok () -> Successful.NO_CONTENT
            | Error ex -> RequestErrors.BAD_REQUEST ex) next ctx
    }

let getAllGamesHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let gamesHandler = Domain.Domain.getAllGames TableStorage.Handlers.getAllGamesFromTableStorage
        let games = gamesHandler()
        return!
            (match games with
            | Ok g -> g |> Seq.map toGameResource |> Successful.OK
            | Error ex -> ServerErrors.INTERNAL_ERROR ex) next ctx
    }

let getGameByIdHandler (id: string): HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let game = Domain.Domain.getGameById TableStorage.Handlers.getGameByIdFromTableStorage id
        return!
            (match game with
            | Ok g -> g |> toGameResource |> Successful.OK
            | Error ex -> ServerErrors.INTERNAL_ERROR ex) next ctx
    }

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Services.AddGiraffe() |> ignore
    let app = builder.Build()

    if app.Environment.IsDevelopment() then
        app.UseDeveloperExceptionPage() |> ignore

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
