module TableStorage

open FSharp.Azure.Storage.Table
open Microsoft.Azure.Cosmos.Table

type Game = {
    [<PartitionKey>] Developer: string
    [<RowKey>] Id: string
    Name: string
    HasMultiplayer: bool }

let account = CloudStorageAccount.Parse "UseDevelopmentStorage=true"
let tableClient = account.CreateCloudTableClient()

let fromGameTable (q: EntityQuery<'a>) = fromTable tableClient "Games" q
let inGameTable game = inTable tableClient "Games" game

let insertGameHandler game =
    let result = game |> Insert |> inGameTable
    if result.HttpStatusCode >= 200 && result.HttpStatusCode < 300 then
        Result.Ok ()
    else
        result.ToString() |> sprintf "Error saving game: %s" |> Result.Error

let getAllGames _ =
    try
        let games = Query.all<Game> |> fromGameTable |> Seq.map fst
        games |> Result.Ok
    with
        | :? System.Exception as ex -> ex.ToString() |> sprintf "Error getting all games: %s" |> Result.Error

let getGameById (id: string) =
    try
        let game =
            Query.all<Game>
            |> Query.where <@ fun g s -> s.RowKey = id @>
            |> fromGameTable
            |> Seq.map fst
            |> Seq.head
        game |> Result.Ok
    with
        | :? System.Exception as ex -> ex.ToString() |> sprintf "Error getting game: %s" |> Result.Error