namespace Portfolio.MongoRepository

open System
open System.Linq
open System.Linq.Expressions
open MongoDB.Driver
open MongoDB.Driver.Linq
open MongoDB.Bson
open MongoDB.Bson.Serialization
open Portfolio.Core
open Portfolio.Core.Entities
open Extensions

type FundRepository (connectionString:string, collectionName:string) =
    inherit CrudRepository<FundAtDate>(connectionString, collectionName, (fun x -> x.Id), FundRepository.overloadMap)

    new(connectionString:string) = FundRepository(connectionString, "Fund")

    member this.CreateFilter<'T> (field:Expression<Func<'T, string>>, value:string):FilterDefinition<'T> =
        FilterDefinitionBuilder<'T>().Eq(field, value)

    static member overloadMap (map:BsonClassMap<FundAtDate>) =         
        map.MapProperty(fun f -> f.LastChangeDate).SetDefaultValue(DateTime(2000, 01, 01)) |> ignore

    interface IFundRepository with
        member this.GetFundsOfCompany(companyId: string): FundAtDate list = 
            let filter = base.Filter().Eq((fun f -> f.FundCompanyId), companyId)
            List.ofSeq (this.Collection.FindSync(filter).ToEnumerable())

        member this.GetFundsOfCurrency(currencyCode: string, limit: int option): FundAtDate list = 
            let filter = base.Filter().Eq((fun f -> f.CurrencyCode), currencyCode)
            let options = FindOptions<FundAtDate>()
            options.Sort <- Builders<FundAtDate>.Sort.Descending((fun f -> f.Date :> obj))
            options.Limit <- Option.toNullable limit
            List.ofSeq (this.Collection.FindSync(filter, options).ToEnumerable())

        member this.GetFundsOfCurrencyLimitedByNumberOfDates(currencyCode: string, limit: int option): obj list = 
            let options = FindOptions<FundAtDate>()
            options.Sort <- Builders<FundAtDate>.Sort.Descending((fun f -> f.Date :> obj))
            options.Limit <- Option.toNullable limit
            //options.Projection <-  ProjectionDefinitionBuilder<FundAtDate>().Expression((fun f -> f.Date)) // BsonDocument("Date", 1)
            let lastDate = this.Collection.FindSync(base.Filter().Empty, options).ToList().Last().Date;


            let aaa = BsonDocument([BsonElement("_id", "$Date"); BsonElement( "count", BsonDocument("$count", BsonDocument()) )])
            let aggregate = [|
                BsonDocument("$match", BsonDocument("CurrencyCode", "DOT"))
                BsonDocument("$group", aaa)
            |]

            let sss = this.Collection.Aggregate(aggregate).ToList()

            []

        member this.GetFundsOfCurrencyGroupedByDate(currencyCode: string, limit: int option): FundAtDate list = 
            // TODO: https://github.com/alex-piccione/portfolio-API-AWS/issues/86
            // use Aggregation to group by date
            // https://www.mongodb.com/docs/drivers/csharp/
            // http://mongodb.github.io/mongo-csharp-driver/2.7/reference/driver/crud/linq/
            // M121 course:  https://university.mongodb.com/mercury/M121/2022_April_5/chapter/Chapter_0_Introduction_and_Aggregation_Concepts/lesson/59ca5aff66d6f7a49c0c4fa5/lecture
            // https://stackoverflow.com/questions/61871149/mongodb-net-group-by-with-list-result-of-whole-objects
            let options = FindOptions<FundAtDate>()
            options.Sort <- Builders<FundAtDate>.Sort.Descending((fun f -> f.Date :> obj))
            options.Limit <- Option.toNullable limit
            //options.Projection <-  ProjectionDefinitionBuilder<FundAtDate>().Expression((fun f -> f.Date)) // BsonDocument("Date", 1)
            let lastDate = this.Collection.FindSync(base.Filter().Empty, options).ToList().Last().Date;

            this.Collection.AsQueryable()
                .Where(fun f -> f.CurrencyCode = currencyCode)
                .GroupBy(fun f -> f.Date)
                .Select(fun x -> new {})

            let filter = base.Filter().Eq((fun f -> f.CurrencyCode), currencyCode)
            let options = FindOptions<FundAtDate>()
            options.Sort <- Builders<FundAtDate>.Sort.Descending((fun f -> f.Date :> obj))
            options.Limit <- Option.toNullable limit

            let aggregateOptions = AggregateOptions()
            //aggregateOptions.Let <- 
            //val create: count:int -> value:'T ->'T[]            

            //let match_ = BsonDocument(dict ["", ""])
            let match_ = BsonDocument("CurrencyCode", currencyCode)
            let currencyFilterStage = BsonDocument(BsonElement("$match", match_))            
            let groupByDateStage = BsonDocument(BsonElement("$group", BsonDocument("Date", 1)))
            let projectDateStage = 
                BsonDocument(BsonElement("$project", 
                    BsonDocument([
                        BsonElement("Date", 1)
                        BsonElement("FundCompanyId", 1)
                        BsonElement("Quantity", 1)]
                )))
            let stages = [|currencyFilterStage; groupByDateStage|] // Array.empty<BsonDocument>           

            //let pipeline = PipelineDefinition<FundAtDate, BsonDocument>.Create(stages)

            let result = this.Collection.Aggregate<BsonDocument>(stages).ToList()
            //this.Collection.Aggregate

            //let group = ProjectionDefinitionBuilder<FundAtDate>().C //BsonDocument("$Date", 1)) //. (BsonDocument("$Date"))
            //let groupByDateProjection = ProjectionDefinition<FundAtDate, DateTime>()
            //let result_ = this.Collection.Aggregate().Group((fun f -> f.Date)).Sort()
                                            //.Group(group)
                    

            for x in result do
                Console.WriteLine(x)

            List.ofSeq (this.Collection.FindSync(filter, options).ToEnumerable())

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