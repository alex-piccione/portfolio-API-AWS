namespace Portfolio.MongoRepository

open System
open System.Linq
open System.Linq.Expressions
open Portfolio.Core
open Portfolio.Core.Entities
open Extensions
open MongoDB.Driver
open MongoDB.Bson.Serialization


type FundRepository (connectionString:string, collectionName:string) =
    inherit CrudRepository<FundAtDate>(connectionString, collectionName, (fun x -> x.Id), FundRepository.overloadMap)

    new(connectionString:string) = FundRepository(connectionString, "Fund")

    member this.CreateFilter<'T> (field:Expression<Func<'T, string>>, value:string):FilterDefinition<'T> =
        FilterDefinitionBuilder<'T>().Eq(field, value)

    static member overloadMap (map:BsonClassMap<FundAtDate>) = 
        map.MapProperty(fun f -> f.LastChangeDate).SetDefaultValue(DateTime(2000, 01, 01)) |> ignore

    interface IFundRepository with
        member this.GetFundsOfCompany(companyId: string): FundAtDate list = 
            let filter = base.Filter().Eq( (fun f -> f.FundCompanyId), companyId)
            List.ofSeq (this.Collection.FindSync(filter).ToEnumerable())

        member this.GetFundsToDate(date: System.DateTime): FundAtDate list = 
            let currencies = List.ofSeq (this.Collection.Distinct((fun x -> x.CurrencyCode), FilterDefinition<FundAtDate>.Empty).ToEnumerable())
            let companies = List.ofSeq (this.Collection.Distinct((fun x -> x.FundCompanyId), FilterDefinition<FundAtDate>.Empty).ToEnumerable())

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