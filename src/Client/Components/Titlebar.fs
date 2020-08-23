namespace Components

module Titlebar =
    open System

    open Elmish
    open Elmish.React
    open Fable.React
    open Fable.React.Props
    open Fetch.Types
    open Thoth.Fetch
    open Thoth.Json
    open Feliz
    open Feliz.UseElmish
    open Feliz.MaterialUI
    open Fable.MaterialUI.Icons
    open Fable.Core.JsInterop


    let createButton (name: string) dispatch (el: ReactElement) =
        Mui.iconButton [
            prop.className name
            prop.onClick (fun _ -> dispatch ())

            iconButton.color.inherit'
            iconButton.children [ el ]
        ]


    let useStyles =
        Styles.makeStyles (fun styles theme ->
            {|
                appBar =
                    styles.create [
                        style.display.flex
                        style.flexDirection.row
                        style.padding (theme.spacing 1)
                        style.zIndex (theme.zIndex.drawer + 1)
                    ]
                menuButton = styles.create [ style.padding (theme.spacing 1) ]
                title = styles.create [ style.padding (theme.spacing 1) ]
            |})


    type Props =
        {
            Title: string
            ButtonsLeft: Button list
            ButtonsRight: Button list
        }

    and Button =
        {
            Button: ReactElement
            Dispatch: unit -> unit
        }

    let private comp =
        React.functionComponent
            ("titlebar",
             (fun (props: Props) ->
                 let classes = useStyles ()

                 let buttonsLeft =
                     props.ButtonsLeft
                     |> List.map (fun b -> createButton classes.menuButton b.Dispatch b.Button)

                 let buttonsRight =
                     props.ButtonsRight
                     |> List.map (fun b -> createButton classes.menuButton b.Dispatch b.Button)

                 Mui.appBar [
                     appBar.classes.root classes.appBar
                     appBar.position.fixed'
                     appBar.children [
                         yield! buttonsLeft
                         // title
                         Mui.typography [
                             prop.className classes.title
                             typography.variant.h6
                             prop.text props.Title
                         ]
                         // filler
                         Html.div [ prop.style [ style.flexGrow 1 ] ]
                         // buttons right
                         yield! buttonsRight
                     ]
                 ]))


    let render title buttonsLeft buttonsRight =

        comp
            ({
                 Title = title
                 ButtonsLeft =
                     buttonsLeft
                     |> List.map (fun (b, d) -> { Button = b; Dispatch = d })
                 ButtonsRight =
                     buttonsRight
                     |> List.map (fun (b, d) -> { Button = b; Dispatch = d })
             })
