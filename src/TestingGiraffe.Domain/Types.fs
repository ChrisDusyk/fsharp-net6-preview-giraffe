namespace Domain

module Types =
    open System

    type Error = {
        Message: string
        InnerException: Exception option
    }
    module Error =
        let create message ex =
            { Message = message
              InnerException = ex }

    type ServiceError =
        | UnexpectedError of Error
        | ValidationError of Error
    module ServiceError =
        let unexpectedErrorFromException message ex : ServiceError = Error.create message ex |> UnexpectedError
        let validationErrorFromException message ex : ServiceError = Error.create message ex |> ValidationError
        let unexpectedError message = Error.create message Option.None |> UnexpectedError
        let validationError message = Error.create message Option.None |> ValidationError

    type Game = {
        Id: Guid
        Developer: string
        Name: string
        HasMultiplayer: bool
    }
    module Game =
        let create (id: string) developer name hasMultiplayer : Result<Game, ServiceError> =
            let success, parsedId = Guid.TryParse id
            if not success then
                id |> sprintf "Error parsing the Id: %s" |> ServiceError.validationError |> Result.Error
            elif String.IsNullOrWhiteSpace(developer) then
                "Empty or invalid developer" |> ServiceError.validationError |> Result.Error
            elif String.IsNullOrWhiteSpace(name) then
                "Empty or invalid name" |> ServiceError.validationError |> Result.Error
            else
                { Id = parsedId
                  Developer = developer
                  Name = name
                  HasMultiplayer = hasMultiplayer } |> Result.Ok
