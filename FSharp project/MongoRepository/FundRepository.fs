namespace Portfolio.MongoRepository

open Portfolio.Core
open Portfolio.Core.Entities

type FundRepository (connectionString:string) =
    inherit CrudRepository<User>(connectionString, "Fund", (fun x -> x.Email))

    interface IFundRepository with
        member this.GetFundsAtDate(date: System.DateTime): FundAtDate list = 
            raise (System.NotImplementedException())
        member this.Save(fund: FundAtDate): unit = 
            raise (System.NotImplementedException())
        