namespace TestingGiraffe.Domain

module Types =
    open System

    type Game = {
        Id: Guid
        Developer: string
        Name: string
        HasMultiplayer: bool
    }
    module Game =
        let create (id: string) developer name hasMultiplayer : Result<Game, string> =
            let success, parsedId = Guid.TryParse id
            if not success then
                id |> sprintf "Error parsing the Id: %s" |> Result.Error
            elif String.IsNullOrWhiteSpace(developer) then
                "Empty or invalid developer" |> Result.Error
            elif String.IsNullOrWhiteSpace(name) then
                "Empty or invalid name" |> Result.Error
            else
                { Id = parsedId
                  Developer = developer
                  Name = name
                  HasMultiplayer = hasMultiplayer } |> Result.Ok