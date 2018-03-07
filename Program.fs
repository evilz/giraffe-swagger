module giraffe_swagger.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open giraffe_swagger.HttpHandlers

open Giraffe
open Giraffe.Swagger
open Giraffe.Swagger.Common
open Giraffe.Swagger.Analyzer
open Giraffe.Swagger.Generator
open Giraffe.Swagger.Dsl
open giraffe_swagger.Models


let docAddendums =
    fun (route:Analyzer.RouteInfos) (path:string,verb:HttpVerb,pathDef:PathDefinition) ->
    
        // routef params are automatically added to swagger, but you can customize their names like this 
        let changeParamName oldName newName (parameters:ParamDefinition list) =
            parameters |> Seq.find (fun p -> p.Name = oldName) |> fun p -> { p with Name = newName }
    
        match path,verb,pathDef with
        | "/parent",_, def ->
            let ndef = 
                { def with Tags=["Tag one"]; }
                    .AddResponse 200 "application/json" "A parent" typeof<Parent>
            path, verb, ndef
        | "/child",_, def ->
            let ndef = 
                { def with Tags=["Tag one"]; }
                    .AddResponse 200 "application/json" "A child" typeof<Child>
            path, verb, ndef

        // | "/articleset", HttpVerb.Post,def ->
        //     let ndef = 
        //         ( { def with Tags=["article set"]; OperationId = "create_article_set" }
        //             .AddConsume "model" "application/json" Body typeof<InputArticleSet>)
        //             .AddResponse 200 "application/json" "Create an article set" typeof<obj>
        //     path, verb, ndef
        

        // | "/", HttpVerb.Get,def ->
        //     // This is another solution to add operation id or other infos
        //     path, verb, { def with OperationId = "Home"; Tags=["home page"] }

        | _ -> path,verb,pathDef

let port = 5000
let docsConfig c = 
    let describeWith desc  = 
        { desc
            with
                Title="Swagger from giraffe"
                Description="test of models"
                TermsOfService=" "
                Version= "0.0.0.1"
        } 
    
    { c with 
        Description = describeWith
        Host = sprintf "localhost:%d" port
        DocumentationAddendums = docAddendums
        MethodCallRules = 
                (fun rules -> 
                    // You can extend quotation expression analysis
                    rules.Add ({ ModuleName="App"; FunctionName="httpFailWith" }, 
                       (fun ctx -> 
                           ctx.AddResponse 500 "text/plain" (typeof<string>)
                )))
    }




// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    swaggerOf (choose [
            GET >=> choose [
                route "/hello" >=> handleGetHello
                route "/parent" >=> handleGetParent
               // route "/child" >=> handleGetChild // when not active child is not in swagger
            ]
            setStatusCode 404 >=> text "Not Found" ]
    ) |> withConfig docsConfig
// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseCors(configureCors)
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .UseIISIntegration()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0