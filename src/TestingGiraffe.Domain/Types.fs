namespace Domain

module Types =
    open System

    type ErrorType = {
        Message: string
        InnerException: Exception option
    }
    module ErrorType =
        let create message ex =
            { Message = message
              InnerException = ex }
        let value e = e

    type ServiceError =
        | UnexpectedError of ErrorType
        | ValidationError of ErrorType
    module ServiceError =
        let unexpectedErrorFromException message ex = ErrorType.create message ex |> UnexpectedError
        let validationErrorFromException message ex = ErrorType.create message ex |> ValidationError
        let unexpectedError message = ErrorType.create message Option.None |> UnexpectedError
        let validationError message = ErrorType.create message Option.None |> ValidationError
        let value serviceError =
            match serviceError with
            | UnexpectedError e -> e
            | ValidationError v -> v

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

module Result =
    let traverseResult f list =
        let (>>=) x f = Result.bind f x
        let retn = Result.Ok

        let cons head tail = head :: tail

        let initState = retn []
        let folder head tail =
            f head >>= (fun h ->
                tail >>= (fun t ->
                    retn (cons h t) ))
        Seq.foldBack folder list initState
