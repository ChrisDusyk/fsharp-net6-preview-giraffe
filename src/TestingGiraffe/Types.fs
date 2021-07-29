module Types

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

type ErrorResponse = {
    Message: string
    StackTrace: string
}

let toErrorResponse (error: Domain.Types.ErrorType): ErrorResponse =
    let stackTrace =
        match error.InnerException with
        | Some e -> e.StackTrace
        | None _ -> ""
    { Message = error.Message
      StackTrace = stackTrace }

type TableStorageConnectionString = TableStorageConnectionString of string
module TableStorageConnectionString =
    let create c = (TableStorageConnectionString c)
    let value (TableStorageConnectionString c) = c