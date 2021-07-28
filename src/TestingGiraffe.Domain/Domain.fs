module Domain

let getAllGames (getAllgamesFromPersistence: unit -> Result<TestingGiraffe.Domain.Types.Game list, string>) =
    let allGames = getAllgamesFromPersistence
    allGames

let getGameById (getGameByIdFromPersistence: (string -> Result<TestingGiraffe.Domain.Types.Game, string>)) (id: string) =
    let game = getGameByIdFromPersistence id
    game

let insertNewGame (insertNewGameIntoPersistence: TestingGiraffe.Domain.Types.Game -> Result<unit, string>) (game: TestingGiraffe.Domain.Types.Game): Result<unit, string> =
    let result = insertNewGameIntoPersistence game
    result