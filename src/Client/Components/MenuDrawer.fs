namespace Components

module MenuDrawer =


    open Elmish
    open Feliz
    open Feliz.UseElmish
    open Feliz.MaterialUI


    type State = string option


    type Msg = MenuItemClick of string


    let init () = None, Cmd.none


    let update dispatch msg state =
        match msg with
        | MenuItemClick s -> Some s, Cmd.ofSub (fun _ -> s |> dispatch)


    let useStyles =
        Styles.makeStyles (fun styles theme ->
            {|
                toolbar = styles.create [ yield! theme.mixins.toolbar ]
            |})

    type Props =
        {
            IsOpen: bool
            Items: string list
            Dispatch: string -> unit
        }


    let private comp =
        React.functionComponent
            ("menudrawer",
             (fun (props: Props) ->
                 let state, dispatch =
                     React.useElmish (init, update props.Dispatch, [||])

                 let classes = useStyles ()

                 Mui.drawer [
                     drawer.open' props.IsOpen
                     drawer.variant.persistent
                     drawer.anchor.left

                     drawer.children [
                         Html.div [ prop.className classes.toolbar ]
                         props.Items
                         |> List.map (fun s ->
                             Mui.listItem [
                                 listItem.button true
                                 match state with
                                 | Some t when t = s -> listItem.selected true
                                 | _ -> listItem.selected false
                                 prop.text s
                                 prop.onClick (fun _ -> s |> MenuItemClick |> dispatch)
                             ])
                         |> Mui.list
                     ]
                 ]))


    let render isOpen dispatch items =
        comp
            ({
                 IsOpen = isOpen
                 Items = items
                 Dispatch = dispatch
             })
