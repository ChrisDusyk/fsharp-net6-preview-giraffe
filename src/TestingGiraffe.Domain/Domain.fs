namespace Domain

module Domain =
    let getAllGames (getAllgamesFromPersistence: unit -> Result<Domain.Types.Game list, string>) =
        let allGames = getAllgamesFromPersistence
        allGames

    let getGameById (getGameByIdFromPersistence: (string -> Result<Domain.Types.Game, string>)) (id: string) =
        let game = getGameByIdFromPersistence id
        game

    let insertNewGame (insertNewGameIntoPersistence: Domain.Types.Game -> Result<unit, string>) (game: Domain.Types.Game): Result<unit, string> =
        let result = insertNewGameIntoPersistence game
        result