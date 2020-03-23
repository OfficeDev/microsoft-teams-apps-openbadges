/*
    <copyright file="badges.tsx" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

import * as React from "react";
import { Text, Menu, Loader, Provider, themes, ShorthandCollection, MenuItemProps } from "@fluentui/react";
import { getAllBadges, getResourceStrings, getMyBadges } from "../api/open-badges-api";
import * as microsoftTeams from "@microsoft/teams-js";
import { ApplicationInsights, SeverityLevel } from "@microsoft/applicationinsights-web";
import { ReactPlugin, withAITracking } from "@microsoft/applicationinsights-react-js";
import { ConfigurationDetails } from "../models/configuration-details";
import { BadgeDetails } from "../models/badge-details";
import { EarnedBadgeDetails } from "../models/earned-badge-details";
import { createBrowserHistory } from "history";
import AllBadges from "./all-badges";
import MyBadges from "./my-badges";
import * as Constants from "../constants";
import "../styles/theme.css";

const moment = require("moment");
const browserHistory = createBrowserHistory({ basename: "" });
let reactPlugin = new ReactPlugin();

/** State interface. */
interface IState {
    isAuthorized: boolean,
    isLoading: boolean,
    resourceStrings: any,
    isResourceStringsLoaded: boolean,
    allBadges: Array<BadgeDetails>,
    myBadges: Array<EarnedBadgeDetails>,
    selectedMenuItemIndex: number,
    menuItems: ShorthandCollection<MenuItemProps, any>,
    theme: string,
    themeStyle: any,
    userBadgrRole?: any
};

/** Component for displaying all badges and user earned badges. */
class Badges extends React.Component<{}, IState>
{
    customAPIAuthenticationToken?: string | null = null;
    state: IState;
    telemetry?: any = null;
    appInsights: ApplicationInsights;
    entityId?: any = null;
    teamId?: any = null;
    userEmail?: any = null;
    theme: string | null = null;
    userObjectId?: string = undefined;
    badgrBaseURL: string | null = null;
    commandContext: string | null = null;

    /**
     * Constructor to initialize component.
     * @param props Props of component.
     */
    constructor(props: {}) {
        super(props);
        this.state = {
            isAuthorized: true,
            isLoading: false,
            resourceStrings: {},
            isResourceStringsLoaded: true,
            allBadges: [],
            myBadges: [],
            selectedMenuItemIndex: 0,
            theme: "",
            themeStyle: themes.teams,
            menuItems: [
                {
                    key: "allbadges",
                    content: "All badges",
                },
                {
                    key: "yourbadges",
                    content: "Your badges",
                }
            ],
            userBadgrRole: null
        };

        let search = window.location.search;
        let params = new URLSearchParams(search);
        this.telemetry = params.get("telemetry");
        this.customAPIAuthenticationToken = params.get("token");
        this.entityId = params.get("entityId");
        this.theme = params.get("theme");
        this.badgrBaseURL = params.get("badgrUrl");
        this.commandContext = params.get("commandContext");

        // Initialize application insights for logging events and errors.
        try {
            this.appInsights = new ApplicationInsights({
                config: {
                    instrumentationKey: this.telemetry,
                    extensions: [reactPlugin],
                    extensionConfig: {
                        [reactPlugin.identifier]: { history: browserHistory }
                    }
                }
            });
            this.appInsights.loadAppInsights();
        }
        catch (e) {
            this.appInsights = new ApplicationInsights({
                config: {
                    instrumentationKey: undefined,
                    extensions: [reactPlugin],
                    extensionConfig: {
                        [reactPlugin.identifier]: { history: browserHistory }
                    }
                }
            });
            console.log(e);
        }

    }

    /** Called once component is mounted. */
    async componentDidMount() {
        microsoftTeams.initialize();
        this.updateTheme(this.theme!);
        this.setState({
            theme: this.theme!
        });

        microsoftTeams.getContext((context) => {
            this.userObjectId = context.userObjectId;
            this.userEmail = context.upn;
            this.teamId = context.channelId;

            this.getResourceStrings();   
            this.getMyBadges();
            this.getAllBadges();
        });

        microsoftTeams.registerOnThemeChangeHandler((theme) => {
            this.updateTheme(theme);
            this.setState({
                theme: theme,
            }, () => {
                this.forceUpdate();
            });
        });
    }

    /**
	* Set current theme state received from teams context
	* @param  {String} theme Current theme name
	*/
    private updateTheme = (theme: string) => {
        if (theme === Constants.DarkTheme) {
            this.setState({
                themeStyle: themes.teamsDark
            });
        } else if (theme === Constants.ContrastTheme) {
            this.setState({
                themeStyle: themes.teamsHighContrast
            });
        } else {
            this.setState({
                themeStyle: themes.teams
            });
        }
    }

    /** 
    *  Get all badges present in issuer group of user.
    * */
    getAllBadges = async () => {
        this.appInsights.trackTrace({ message: `'getAllBadges' - Initiated request`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        this.setState({ isResourceStringsLoaded: false });
        let allBadgesResponse = await getAllBadges(this.customAPIAuthenticationToken!, this.userEmail);
        if (allBadgesResponse) {
            this.setState({ isResourceStringsLoaded: true });

            if (allBadgesResponse.status === 200) {
                let allBadges = allBadgesResponse.data.allBadges;

                if (allBadges !== null) {
                    this.setState({ allBadges: allBadges, userBadgrRole: allBadgesResponse.data.userBadgrRole });
                }
                else {
                    this.appInsights.trackTrace({ message: `'getAllBadges' - Response is null`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Warning });
                }
            }
            else {
                this.appInsights.trackTrace({ message: `'getAllBadges' - Request failed:${allBadgesResponse.status}`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Warning });
            }
        }
    }

    /** 
   *  Get resource strings according to user locale.
   * */
    getResourceStrings = async () => {
        this.appInsights.trackTrace({ message: `'getResourceStrings' - Initiated request`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        const resourceStringsResponse = await getResourceStrings(this.customAPIAuthenticationToken!);
        if (resourceStringsResponse) {
            if (resourceStringsResponse.status === 200) {
                let menuItems = [
                    {
                        key: "allbadges",
                        content: resourceStringsResponse.data.AllBadges,
                    },
                    {
                        key: "yourbadges",
                        content: resourceStringsResponse.data.YourBadges,
                    }
                ];
                this.setState({ resourceStrings: resourceStringsResponse.data, menuItems: menuItems });
            }
            else {
                this.appInsights.trackTrace({ message: `'getResourceStrings' - Request failed:${resourceStringsResponse.status}`, severityLevel: SeverityLevel.Error, properties: { User: this.userObjectId, Code: resourceStringsResponse.status } });
            }
        }
    }

    /** 
   *  Get badges awarded to user.
   * */
    getMyBadges = async () => {
        this.appInsights.trackTrace({ message: `'getMyBadges' - Initiated request`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        const myBadgesResponse = await getMyBadges(this.customAPIAuthenticationToken!);
        if (myBadgesResponse) {
            if (myBadgesResponse.status === 200) {
                myBadgesResponse.data.map((value: any) => (value.AwardedOn = moment(value.AwardedOn).format("D MMM YYYY")));
                this.setState({ myBadges: myBadgesResponse.data });
            }
            else {
                this.appInsights.trackTrace({ message: "getMyBadges - Request failed", severityLevel: SeverityLevel.Error, properties: { User: this.userObjectId, Code: myBadgesResponse.status } });
            }
        }
    }

    /** 
    *  Called once menu item is clicked.
    * */
    onMenuItemClick = (event: any, data: any) => {
        this.setState({ selectedMenuItemIndex: data.index });
    }

    /** 
    *  Called once create badge icon is clicked.
    * */
    onCreateBadgeClick = (event: any) => {
        this.appInsights.trackEvent({ name: `Create badge` }, { User: this.userObjectId });
        window.location.href = Constants.BadgrCreateBadgeURL.replace("{entityId}", this.entityId);
    }

    /** Render function. */
    render() {
        /** Return content based on selected menu item. */
        const showMenuContent = (): JSX.Element => {
            if (this.state.selectedMenuItemIndex === 0) {
                const configurationDetails = {
                    Token: this.customAPIAuthenticationToken,
                    Telemetry: this.telemetry,
                    Role: this.state.userBadgrRole,
                    EntityId: this.entityId,
                    TeamId: this.teamId,
                    Theme: this.theme,
                    CommandContext: this.commandContext
                } as ConfigurationDetails;

                let showCreateBadge = false;
                if (configurationDetails.Role !== Constants.StaffRole && configurationDetails.Role !== null) {
                    showCreateBadge = true;
                }

                let className = "badge-default";
                if (this.state.theme === "contrast") {
                    className = "badge-contrast";
                }
                else if (this.state.theme === "dark") {
                    className = "badge-dark";
                }

                return (<AllBadges allBadges={this.state.allBadges} onCreateBadgeClick={this.onCreateBadgeClick} resourceStrings={this.state.resourceStrings} showCreateBadge={showCreateBadge} configurationDetails={configurationDetails} userEmail={this.userEmail} backgroundClassName={className} />);
            }
            else {
                return (<MyBadges myBadges={this.state.myBadges} resourceStrings={this.state.resourceStrings} />);
            }
        }

        /** Check if resource strings are fetched from API. */
        const renderPageContent = (): JSX.Element => {
            if (this.state.isResourceStringsLoaded) {
                return (
                    <Provider theme={this.state.themeStyle}>
                        <div className="container-div">
                            <Menu defaultActiveIndex={0} onItemClick={this.onMenuItemClick} items={this.state.menuItems} styles={{ borderBottom: "0", marginBottom: "1rem", marginTop: "0.5rem" }} underlined primary />
                            {this.state.selectedMenuItemIndex === 0 && <Text content={this.state.resourceStrings.SelectBadge} />}
                            <div className="container-subdiv">
                                {this.state.isLoading === true ? <Loader /> : showMenuContent()}
                            </div>
                        </div>
                    </Provider>
                );
            }
            else {
                return (
                    <Provider theme={this.state.themeStyle}>
                        <div className="container-div">
                            <Loader />
                        </div>
                    </Provider>
                );
            }
        }

        return (renderPageContent());
    }
}

export default withAITracking(reactPlugin, Badges);