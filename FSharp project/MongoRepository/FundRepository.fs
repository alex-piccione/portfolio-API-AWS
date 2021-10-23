namespace Portfolio.MongoRepository

open System.Linq
open Portfolio.Core
open Portfolio.Core.Entities
open MongoDB.Driver

type FundRepository (connectionString:string) =
    inherit CrudRepository<FundAtDate>(connectionString, "Fund", (fun x -> x.Id))

    (*interface IFundRepository with
        member this.GetFundsToDate(date: System.DateTime): FundAtDate list = 
            // group by currency and get the latest
            let aggregate = base.Collection.Aggregate().Group((fun f -> f.CurrencyCode), fun g -> g.Where(fun g -> g.Date <= date).OrderBy(fun f -> f.Date).Last() )
            List.ofSeq( aggregate.ToEnumerable() )
            *)

    interface IFundRepository with
        member this.GetFundsToDate(date: System.DateTime): FundAtDate list = 

            let currencies = List.ofSeq (this.Collection.Distinct((fun x -> x.CurrencyCode), FilterDefinition<FundAtDate>.Empty).ToEnumerable())

            let getLast currencyCode = Seq.tryHead( this.Collection.FindSync((fun f -> f.CurrencyCode = currencyCode && f.Date <= date))
                                           .ToEnumerable().OrderByDescending( (fun f -> f.Date)))

            currencies |> List.choose (fun c -> getLast c)
