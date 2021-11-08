namespace Portfolio.MongoRepository

open System.Linq
open Portfolio.Core
open Portfolio.Core.Entities
open Extensions
open MongoDB.Driver
open System.Linq.Expressions
open System

type FundRepository (connectionString:string, collectionName:string) =
    inherit CrudRepository<FundAtDate>(connectionString, collectionName, (fun x -> x.Id))

    new(connectionString:string) = FundRepository(connectionString, "Fund")

    (*interface IFundRepository with
        member this.GetFundsToDate(date: System.DateTime): FundAtDate list = 
            // group by currency and get the latest
            let aggregate = base.Collection.Aggregate().Group((fun f -> f.CurrencyCode), fun g -> g.Where(fun g -> g.Date <= date).OrderBy(fun f -> f.Date).Last() )
            List.ofSeq( aggregate.ToEnumerable() )
            *)

    member this.CreateFilter<'T> (field:Expression<Func<'T, string>>, value:string):FilterDefinition<'T> =
        FilterDefinitionBuilder<'T>().Eq(field, value)

    interface IFundRepository with

        member this.GetFundsToDate(date: System.DateTime): FundAtDate list = 

            let currencies = List.ofSeq (this.Collection.Distinct((fun x -> x.CurrencyCode), FilterDefinition<FundAtDate>.Empty).ToEnumerable())

            let getLast currencyCode = Seq.tryHead( this.Collection.FindSync((fun f -> f.CurrencyCode = currencyCode && f.Date <= date))
                                           .ToEnumerable().OrderByDescending( (fun f -> f.Date)))

            currencies |> List.choose (fun c -> getLast c)

        (*
        member this.InsertOrUpdateFundAtDate(record: FundAtDate): unit = 
            
            let byDate = base.Filter().Eq((fun f -> f.Date), record.Date)
            let byCurrency = base.Filter().Eq((fun f -> f.CurrencyCode), record.CurrencyCode)
            let byCompany = base.Filter().Eq((fun f -> f.FundCompanyId), record.FundCompanyId)
            let filter = base.Filter().And (byDate, byCurrency, byCompany)

            let filter_2 = 
                FilterDefinitionBuilder<FundAtDate>().And(
                    FilterDefinitionBuilder<FundAtDate>().Eq((fun f -> f.Date), record.Date),
                    FilterDefinitionBuilder<FundAtDate>().Eq((fun f -> f.CurrencyCode), record.CurrencyCode), 
                    FilterDefinitionBuilder<FundAtDate>().Eq((fun f -> f.FundCompanyId), record.FundCompanyId)
                )
            let options = UpdateOptions()
            options.IsUpsert <- true
            let update = UpdateDefinitionBuilder<FundAtDate>()
                            .Set((fun f -> f.Quantity), record.Quantity)
          
            let result = this.Collection.UpdateOne(filter, update, options)

            raise (System.NotImplementedException())
         *)

         member this.FindFundAtDate(record: FundAtDate): FundAtDate option = 
             let byDate = base.Filter().Eq((fun f -> f.Date), record.Date)
             let byCurrency = base.Filter().Eq((fun f -> f.CurrencyCode), record.CurrencyCode)
             let byCompany = base.Filter().Eq((fun f -> f.FundCompanyId), record.FundCompanyId)
             let filter = base.Filter().And (byDate, byCurrency, byCompany)

             (*let f = (fun (f:FundAtDate) -> 
                 f.CurrencyCode = record.CurrencyCode 
                 && f.Date = record.Date 
                 && f.FundCompanyId = record.FundCompanyId)*)

             this.Collection.FindOneOrNone(filter)

         member this.CreateFundAtDate(record: FundAtDate): unit =
             base.Create(record)

         member this.UpdateFundAtDate (record: FundAtDate): unit = 
             base.Update(record.Id, record)