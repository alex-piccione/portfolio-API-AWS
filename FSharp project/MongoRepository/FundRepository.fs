namespace Portfolio.MongoRepository

open System
open System.Linq
open System.Linq.Expressions
open MongoDB.Driver
open MongoDB.Bson.Serialization
open Portfolio.Core
open Portfolio.Core.Entities
open Extensions

type FundRepository (config:DatabaseConfig, collectionName:string) =
    inherit CrudRepository<FundAtDate>(config, collectionName, (fun x -> x.Id), FundRepository.overloadMap)

    new(config:DatabaseConfig) = FundRepository(config, "Fund")

    member this.CreateFilter<'T> (field:Expression<Func<'T, string>>, value:string):FilterDefinition<'T> =
        FilterDefinitionBuilder<'T>().Eq(field, value)

    static member overloadMap (map:BsonClassMap<FundAtDate>) =         
        map.MapProperty(fun f -> f.LastChangeDate).SetDefaultValue(DateTime(2000, 01, 01)) |> ignore

    interface IFundRepository with
        member this.GetFundsOfCompany(companyId: string): FundAtDate list = 
            let filter = base.Filter().Eq((fun f -> f.FundCompanyId), companyId)
            List.ofSeq (this.Collection.FindSync(filter).ToEnumerable())

        member this.GetFundsOfCurrency(currencyCode: string, minDate: DateTime): FundAtDate list =       
            List.ofSeq(this.Collection.FindSync(fun f -> f.CurrencyCode = currencyCode && f.Date >= minDate).ToEnumerable())

        member this.GetFundsToDate(date: System.DateTime): FundAtDate list = 
            //  TODO: can be done in parallel?
            let currencies = List.ofSeq (this.Collection.Distinct((fun x -> x.CurrencyCode), FilterDefinition<FundAtDate>.Empty).ToEnumerable())
            let companies = List.ofSeq (this.Collection.Distinct((fun x -> x.FundCompanyId), FilterDefinition<FundAtDate>.Empty).ToEnumerable())

            // TODO: check $lookup
            // https://youtu.be/nyKQagiYZlI?t=747

            let getLast currencyCode companyId = 
                Seq.tryHead( this.Collection.FindSync((fun f -> f.CurrencyCode = currencyCode && f.FundCompanyId = companyId && f.Date <= date))
                    .ToEnumerable().OrderByDescending((fun f -> f.Date)))            

            List.allPairs currencies companies |> List.choose (fun x -> getLast (fst(x)) (snd(x)))

         member this.FindFundAtDate(record: FundAtDate): FundAtDate option = 
             this.Collection.FindOneOrNone(
                (fun f -> f.Date = record.Date 
                        && f.CurrencyCode = record.CurrencyCode
                        && f.FundCompanyId = record.FundCompanyId
                ))

         member this.CreateFundAtDate(record: FundAtDate): unit =
             base.Create(record)

         member this.UpdateFundAtDate (record: FundAtDate): unit = 
             base.Update(record.Id, record)