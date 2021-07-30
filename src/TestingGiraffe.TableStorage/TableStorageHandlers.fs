namespace TableStorage

module Handlers =
    open FSharp.Azure.Storage.Table
    open Microsoft.Azure.Cosmos.Table
    open TableStorage.Types
    open Domain

    let [<Literal>] private gamesTableName = "Games"

    let private fromGameTable tableClient (query: EntityQuery<'a>) = fromTable tableClient gamesTableName query
    let private inGameTable tableClient game = inTable tableClient gamesTableName game

    let private toDomainGame (tableStorageGame: Game) =
        Domain.Types.Game.create tableStorageGame.Id tableStorageGame.Developer tableStorageGame.Name tableStorageGame.HasMultiplayer
    let private toTableStorageGame (domainGame: Domain.Types.Game) : Game =
        { Id = domainGame.Id.ToString()
          Developer = domainGame.Developer
          Name = domainGame.Name
          HasMultiplayer = domainGame.HasMultiplayer }

    let private getAllGamesFromTableStorage (tableClient: CloudTableClient) =
        fun _ ->
            try
                let games = Query.all<Game> |> fromGameTable tableClient |> Seq.map fst
                games |> Result.traverseResult toDomainGame
            with
                | :? System.Exception as ex ->
                    let message = sprintf "Error retrieving all games: %s" ex.Message
                    Domain.Types.ServiceError.unexpectedErrorFromException message (Option.Some ex) |> Result.Error

    let private getGameByIdFromTableStorage (tableClient: CloudTableClient) (id: string) =
        try
            let game =
                Query.all<Game>
                |> Query.where <@ fun g s -> s.RowKey = id @>
                |> fromGameTable tableClient
                |> Seq.map fst
                |> Seq.head
                |> toDomainGame
            game
        with
            | :? System.Exception as ex ->
                let message = sprintf "Error getting game %s: %s" id ex.Message
                Domain.Types.ServiceError.unexpectedErrorFromException message (Option.Some ex) |> Result.Error

    let private insertGameIntoTableStorage (tableClient: CloudTableClient) (game: Domain.Types.Game) =
        let result = game |> toTableStorageGame |> Insert |> inGameTable tableClient
        if result.HttpStatusCode >= 200 && result.HttpStatusCode < 300 then
            Result.Ok ()
        else
            result.ToString() |> sprintf "Error saving game: %s" |> Domain.Types.ServiceError.unexpectedError |> Result.Error

    let composeInsertGameHandler tableClient =
        insertGameIntoTableStorage tableClient

    let composeGetAllGamesHandler tableClient =
        getAllGamesFromTableStorage tableClient

    let composeGetGameByIdHandler tableClient =
        getGameByIdFromTableStorage tableClient