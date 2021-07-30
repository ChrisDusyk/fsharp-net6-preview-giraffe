namespace Domain

open Domain.Types

module Domain =
    let composeGetAllGames (getAllgamesFromPersistence: unit -> Result<Game list, ServiceError>) =
        let getAllGames (getAllgamesFromPersistence: unit -> Result<Game list, ServiceError>) =
            let allGames = getAllgamesFromPersistence
            allGames
        getAllGames getAllgamesFromPersistence

    let composeGetGameById (getGameByIdFromPersistence: (string -> Result<Game, ServiceError>)) =
        let getGameById (getGameByIdFromPersistence: (string -> Result<Game, ServiceError>)) (id: string) =
            let game = getGameByIdFromPersistence id
            game
        getGameById getGameByIdFromPersistence

    let composeInsertNewGame (insertNewGameIntoPersistence: Game -> Result<unit, ServiceError>) =
        let insertNewGame (insertNewGameIntoPersistence: Game -> Result<unit, ServiceError>) (game: Domain.Types.Game): Result<unit, ServiceError> =
            let result = insertNewGameIntoPersistence game
            result
        insertNewGame insertNewGameIntoPersistence