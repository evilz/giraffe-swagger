namespace giraffe_swagger

open System
module HttpHandlers =

    open Microsoft.AspNetCore.Http
    open Giraffe
    open giraffe_swagger.Models

    let handleGetHello =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let response = {
                    Text = "Hello world, from Giraffe!"
                }
                return! json response next ctx
            }

    

    let handleGetParent =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let response = {
                                date= DateTime.Now
                                child= {
                                        prop1= "the fox" 
                                        prop2= 42
                                }
                            }
                return! json response next ctx
            }


    let handleGetChild =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let response =  {
                                        prop1= "the fox" 
                                        prop2= 42
                                }
                return! json response next ctx
            }