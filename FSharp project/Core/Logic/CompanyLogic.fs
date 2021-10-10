namespace Portfolio.Core.Logic

open System
open Portfolio.Core
open Portfolio.Core.Entities

type ICompanyLogic =
    abstract member Create:Company -> Company
    abstract member Update:Company -> unit

type CompanyLogic(companyRepository:ICompanyRepository) =
    
    let assignNewId company = {company with Id=Guid.NewGuid().ToString()}

    let normalize company = {company with Name=company.Name.Trim()}

    let validate company = 
        if String.IsNullOrWhiteSpace(company.Name) then failwith "\"Name\" cannot be empty."
        company

    interface ICompanyLogic with
        member this.Create(company:Company) =
            let newCompany = normalize company |> validate |> assignNewId
            companyRepository.Create(newCompany)
            newCompany
            
        member this.Update (company:Company) =
            let updateCompany = normalize company
            companyRepository.Update updateCompany