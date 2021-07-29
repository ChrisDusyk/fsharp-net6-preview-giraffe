namespace TableStorage

module Handlers =
    open FSharp.Azure.Storage.Table
    open Microsoft.Azure.Cosmos.Table
    open TableStorage.Types

    let [<Literal>] private gamesTableName = "Games"

    let private account = CloudStorageAccount.Parse "UseDevelopmentStorage=true"
    let private tableClient = account.CreateCloudTableClient()

    let private fromGameTable (query: EntityQuery<'a>) = fromTable tableClient gamesTableName query
    let private inGameTable game = inTable tableClient gamesTableName game

    let private toDomainGame (tableStorageGame: Game) =
        Domain.Types.Game.create tableStorageGame.Id tableStorageGame.Developer tableStorageGame.Name tableStorageGame.HasMultiplayer
    let private toTableStorageGame (domainGame: Domain.Types.Game) : Game =
        { Id = domainGame.Id.ToString()
          Developer = domainGame.Developer
          Name = domainGame.Name
          HasMultiplayer = domainGame.HasMultiplayer }

    let getAllGamesFromTableStorage =
        fun _ ->
            try
                let games = Query.all<Game> |> fromGameTable |> Seq.map fst
                games |> Result.traverseResult toDomainGame
            with
                | :? System.Exception as ex -> ex.ToString() |> sprintf "Error getting all games: %s" |> Result.Error

    let getGameByIdFromTableStorage (id: string) =
        try
            let game =
                Query.all<Game>
                |> Query.where <@ fun g s -> s.RowKey = id @>
                |> fromGameTable
                |> Seq.map fst
                |> Seq.head
                |> toDomainGame
            game
        with
            | :? System.Exception as ex -> ex.ToString() |> sprintf "Error getting game: %s" |> Result.Error

    let insertGameIntoTableStorage (game: Domain.Types.Game) =
        let result = game |> toTableStorageGame |> Insert |> inGameTable
        if result.HttpStatusCode >= 200 && result.HttpStatusCode < 300 then
            Result.Ok ()
        else
            result.ToString() |> sprintf "Error saving game: %s" |> Result.Error