﻿namespace Portfolio.MongoRepository

open System
open System.Linq
open System.Linq.Expressions
open Portfolio.Core
open Portfolio.Core.Entities
open Extensions
open MongoDB.Driver


type FundRepository (connectionString:string, collectionName:string) =
    inherit CrudRepository<FundAtDate>(connectionString, collectionName, (fun x -> x.Id))

    new(connectionString:string) = FundRepository(connectionString, "Fund")

    member this.CreateFilter<'T> (field:Expression<Func<'T, string>>, value:string):FilterDefinition<'T> =
        FilterDefinitionBuilder<'T>().Eq(field, value)

    interface IFundRepository with

        member this.GetFundsToDate(date: System.DateTime): FundAtDate list = 

            let currencies = List.ofSeq (this.Collection.Distinct((fun x -> x.CurrencyCode), FilterDefinition<FundAtDate>.Empty).ToEnumerable())

            let getLast currencyCode = Seq.tryHead( this.Collection.FindSync((fun f -> f.CurrencyCode = currencyCode && f.Date <= date))
                                           .ToEnumerable().OrderByDescending( (fun f -> f.Date)))

            currencies |> List.choose (fun c -> getLast c)

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