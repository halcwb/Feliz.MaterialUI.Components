namespace Components

module RadioButtons =
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



    let useStyles =
        Styles.makeStyles (fun styles theme ->
            {| fieldset =
                   styles.create
                       [ style.marginTop (theme.spacing 3)
                         style.marginBottom (theme.spacing 3) ] |})


    type Props =
        { Title: string
          Items: Item list
          Dispatch: string -> unit }

    and Item =
        { Value: string
          Label: string }


    let props =
        { Title = ""
          Items = []
          Dispatch = ignore }


    let private comp =
        React.functionComponent
            ("radiobuttons",
             (fun (props: Props) ->
                 let classes = useStyles()

                 Mui.formControl
                     [ formControl.component' "fieldset"
                       prop.className classes.fieldset
                       formControl.children
                           [ Mui.formLabel
                               [ formLabel.component' "legend"
                                 prop.text props.Title ]
                             Mui.radioGroup
                                 [ radioGroup.onChange (props.Dispatch)
                                   // create the radiobuttons
                                   props.Items
                                   |> List.map (fun b ->
                                       Mui.formControlLabel
                                           [ formControlLabel.control (Mui.radio [])
                                             formControlLabel.value b.Value
                                             formControlLabel.label b.Label ])

                                   |> radioGroup.children ] ] ])

            )

    let render title dispatch items =
        let items =
            items
            |> List.map (fun (v, l) ->
                { Value = v
                  Label = l })
        comp
            ({ Title = title
               Dispatch = dispatch
               Items = items })
