namespace Domain

open Domain.Types

module Domain =
    let getAllGames (getAllgamesFromPersistence: unit -> Result<Game list, ServiceError>) =
        let allGames = getAllgamesFromPersistence
        allGames

    let getGameById (getGameByIdFromPersistence: (string -> Result<Game, ServiceError>)) (id: string) =
        let game = getGameByIdFromPersistence id
        game

    let insertNewGame (insertNewGameIntoPersistence: Game -> Result<unit, ServiceError>) (game: Domain.Types.Game): Result<unit, ServiceError> =
        let result = insertNewGameIntoPersistence game
        result