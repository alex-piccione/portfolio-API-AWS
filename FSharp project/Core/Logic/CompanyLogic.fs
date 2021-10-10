namespace Portfolio.Core.Logic

open System
open Portfolio.Core
open Portfolio.Core.Entities


type CompanyLogic(companyRepository:ICompanyRepository) =

    let normalize company =
        {company with Id=Guid.NewGuid().ToString(); Name=company.Name.Trim()}

    let validate company = 
        if String.IsNullOrWhiteSpace( company.Name) then failwith "\"Name\" cannot be empty."
        company

    member this.Create(company:Company) =

        let newCompany = validate <| normalize company
        companyRepository.Create(newCompany)
