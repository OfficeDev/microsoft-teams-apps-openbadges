/*
    <copyright file="award-badge.tsx" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

import * as React from "react";
import { RouteComponentProps } from "react-router";
import { createBrowserHistory } from "history";
import { submitAwardBadge, getResourceStrings, getMembersInTeam } from "../api/open-badges-api";
import { Text, Flex, Loader, Input, Dropdown, TextArea, Provider, themes } from "@fluentui/react";
import { ApplicationInsights, SeverityLevel } from "@microsoft/applicationinsights-web";
import { ReactPlugin, withAITracking } from "@microsoft/applicationinsights-react-js";
import * as microsoftTeams from "@microsoft/teams-js";
import AwardBadgeFooter from "./footer";
import PreviewBadge from "./preview-award"
import { BadgeAwardPreview } from "../models/badge-award-preview"
import { BadgeAward } from "../models/badge-award"
import { ConfigurationDetails } from "../models/configuration-details"
import * as Constants from "../constants";
import "../styles/theme.css";

const browserHistory = createBrowserHistory({ basename: "" });
let reactPlugin = new ReactPlugin();

interface IBadgesProps extends RouteComponentProps {
    BadgeAward: BadgeAward,
    ConfigurationDetails: ConfigurationDetails
};

/** State interface. */
interface IState {
    isLoading: boolean,
    resourceStrings: any,
    resourceStringsLoaded: boolean,
    allMembers: Array<any>,
    selectedMembers: Array<any>,
    selectedBadge: string | null,
    note: string | null,
    errorMessage: string | null,
    isPreviewBadge: boolean,
    awardedByName: string | null,
    awardedByEmail: string | null,
    theme: string | null,
    themeStyle: any;
    isAwardBadgeLoading: boolean
};

/** Component which allows user to fill details needed to award a badge to user(s). */
class AwardBadge extends React.Component<IBadgesProps, IState>
{
    customAPIAuthenticationToken?: string | null = null;
    state: IState;
    telemetry: any = undefined;
    appInsights: ApplicationInsights;
    userBadgrRole: string | null = null;
    entityId: string | null = null;
    selectedBadge: string | null = null;
    teamId: string | null = null;
    badgeImage: string | null = null;
    badgeId: string | null = null;
    awardedByEmail: string | null = null;
    theme: string | null = null;
    badgeDescription: string | null = null;
    badgeCriteria: string | null = null;
    badgeCriteriaUrl: string | null = null;
    commandContext: string | null = null;
    userObjectId?: string = undefined;

    /**
     * Constructor to initialize component.
     * @param props Props of component.
     */
    constructor(props: IBadgesProps) {
        super(props);
        this.state = {
            isLoading: false,
            resourceStrings: {},
            resourceStringsLoaded: false,
            allMembers: [],
            selectedMembers: [],
            selectedBadge: null,
            note: null,
            errorMessage: null,
            isPreviewBadge: false,
            awardedByName: null,
            awardedByEmail: null,
            theme: null,
            themeStyle: themes.teams,
            isAwardBadgeLoading: false
        };

        const badgeProps = this.props.location.state as IBadgesProps;
        this.telemetry = badgeProps.ConfigurationDetails.Telemetry;
        this.customAPIAuthenticationToken = badgeProps.ConfigurationDetails.Token;
        this.userBadgrRole = badgeProps.ConfigurationDetails.Role;
        this.entityId = badgeProps.ConfigurationDetails.EntityId;
        this.commandContext = badgeProps.ConfigurationDetails.CommandContext;
        this.teamId = badgeProps.ConfigurationDetails.TeamId;
        this.selectedBadge = badgeProps.BadgeAward.BadgeName;
        this.badgeImage = badgeProps.BadgeAward.ImageUrl;
        this.awardedByEmail = badgeProps.BadgeAward.AwardedBy;
        this.badgeId = badgeProps.BadgeAward.BadgeId;
        this.theme = badgeProps.ConfigurationDetails.Theme;
        this.badgeDescription = badgeProps.BadgeAward.BadgeDescription;
        this.badgeCriteria = badgeProps.BadgeAward.Criteria;
        this.badgeCriteriaUrl = badgeProps.BadgeAward.CriteriaUrl;

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
        catch (exception) {
            this.appInsights = new ApplicationInsights({
                config: {
                    instrumentationKey: undefined,
                    extensions: [reactPlugin],
                    extensionConfig: {
                        [reactPlugin.identifier]: { history: browserHistory }
                    }
                }
            });
            console.log(exception);
        }

    }

    /** Called once component is mounted. */
    componentDidMount() {
        microsoftTeams.initialize();
        this.updateTheme(this.theme!);
        this.setState({
            theme: this.theme!
        });

        microsoftTeams.getContext((context) => {
            this.userObjectId = context.userObjectId;
        });

        microsoftTeams.registerOnThemeChangeHandler((theme) => {
            this.updateTheme(theme);
            this.setState({
                theme: theme,
            }, () => {
                this.forceUpdate();
            });
        });
        this.getResourceStrings();
        this.getMembersInTeam();
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
    *  Get all team members.
    * */
    getMembersInTeam = async () => {
        this.appInsights.trackTrace({ message: `'getMembersInTeam' - Request initiated`, severityLevel: SeverityLevel.Information, properties: { User: this.userObjectId} });
        this.setState({ isLoading: true });
        const teamMemberResponse = await getMembersInTeam(this.teamId!, this.customAPIAuthenticationToken!);
        if (teamMemberResponse) {
            if (teamMemberResponse.status === 200) {
                // Remove user who is awarding badge from the drop down. 
                var user = teamMemberResponse.data.find((member, index, obj) => { return member.content.toUpperCase() === this.awardedByEmail!.toUpperCase() });
                teamMemberResponse.data.splice(teamMemberResponse.data.indexOf(user), 1);
                this.setState({ allMembers: teamMemberResponse.data, selectedBadge: this.selectedBadge, awardedByName: user.header.split("(")[0], awardedByEmail: user.content });
            }
            else {
                this.appInsights.trackTrace({ message: `'getMembersInTeam' - Request failed:${teamMemberResponse.status}`, severityLevel: SeverityLevel.Error, properties: { User: this.userObjectId, Code: teamMemberResponse.status } });
            }
        }
        this.setState({ isLoading: false });
    }

    /** 
   *  Get resource strings according to user locale.
   * */
    getResourceStrings = async () => {
        this.appInsights.trackTrace({ message: `'getResourceStrings' - Request initiated`, severityLevel: SeverityLevel.Information, properties: { User: this.userObjectId } });
        const resourceStringsResponse = await getResourceStrings(this.customAPIAuthenticationToken!);
        if (resourceStringsResponse) {
            this.setState({ resourceStringsLoaded: true });

            if (resourceStringsResponse.status === 200) {
                this.setState({ resourceStrings: resourceStringsResponse.data });
            }
            else {
                this.appInsights.trackTrace({ message: `'getResourceStrings' - Request failed:${resourceStringsResponse.status}`, severityLevel: SeverityLevel.Error, properties: { User: this.userObjectId, Code: resourceStringsResponse.status } });
            }
        }
    }

    /**
    *  Handles logic when user clicks on award button.
    * */
    submitAwardBadge = async () => {
        if (this.state.selectedMembers.length === 0) {
            this.setState({ errorMessage: this.state.resourceStrings.SelectAtleastOneMember });
            return;
        }

        if (this.state.note) {
            if (this.state.note!.length > 250) {
                this.setState({ errorMessage: this.state.resourceStrings.NoteCharacterLimitExceeded });
                return;
            }
        }        

        if (this.state.errorMessage !== null) {
            this.setState({ errorMessage: null });
        }

        this.setState({ isAwardBadgeLoading: true });

        let assertions: Array<any> = [];

        this.state.selectedMembers.forEach((value) => {
            assertions.push({ recipient_identifier: value.content, narrative: this.state.note });
        });

        let assertionDetails = { issuer: null, badge_class: this.badgeId, create_notification: false, assertions: assertions };
        this.appInsights.trackEvent({ name: `Award badge` }, { User: this.userObjectId });
        this.appInsights.trackTrace({ message: `'submitAwardBadge' - Request initiated`, severityLevel: SeverityLevel.Information, properties: { User: this.userObjectId } });
        const awardBadgeResponse = await submitAwardBadge(assertionDetails, this.customAPIAuthenticationToken!);
        if (awardBadgeResponse) {
            if (awardBadgeResponse.status === 200) {
                let memberEmails = assertions.map((value: any, index) => (
                    value.recipient_identifier
                ));
                let toBot = { AwardedBy: this.state.awardedByEmail, ImageUri: this.badgeImage, BadgeName: this.selectedBadge, AwardRecipients: memberEmails, Narrative: this.state.note, CommandContext: this.commandContext };
                microsoftTeams.tasks.submitTask(toBot);
            }
            else {
                this.setState({ isAwardBadgeLoading: false, errorMessage: this.state.resourceStrings.ExceptionResponse });
                this.appInsights.trackTrace({ message: `'submitAwardBadge' - Request failed`, severityLevel: SeverityLevel.Error, properties: { User: this.userObjectId, Code: awardBadgeResponse.status } });
            }
        }
    }

    /**
    *  Handles multi select drop-down changes.
    * */
    onMemberSelectionChanged = {
        onAdd: item => {
            let selectedMembers = this.state.selectedMembers;
            selectedMembers.push(item);
            this.setState({ selectedMembers: selectedMembers });
            return "";
        },
        onRemove: item => {
            let selectedMembers = this.state.selectedMembers;
            selectedMembers.splice(selectedMembers.indexOf(item), 1);
            this.setState({ selectedMembers: selectedMembers });
            return "";
        },
    }

    /**
    *  Handles logic when user clicks on preview button.
    * */
    onPreviewClick = () => {
        if (this.state.selectedMembers.length === 0) {
            this.setState({ errorMessage: this.state.resourceStrings.SelectAtleastOneMember });
            return;
        }

        if (this.state.note) {
            if (this.state.note!.length > 250) {
                this.setState({ errorMessage: this.state.resourceStrings.NoteCharacterLimitExceeded });
                return;
            }
        }

        if (this.state.errorMessage !== null) {
            this.setState({ errorMessage: null });
        }

        this.setState({ isPreviewBadge: true });
    }

    /**
    *  Handles logic when user clicks on back button.
    * */
    onBackClick = () => {
        if (this.state.isPreviewBadge) {
            this.setState({ selectedMembers: this.state.selectedMembers });
            this.setState({ isPreviewBadge: false });
        }
        else {
            window.location.href = window.location.origin + "/AllBadges?token=" + this.customAPIAuthenticationToken + "&telemetry=" + this.telemetry + "&role=" + this.userBadgrRole + "&entityId=" + this.entityId + "&badge=" + this.selectedBadge + "&theme=" + this.theme + "&commandContext=" + this.commandContext;
        }

    }

    /**
    *  Returns layout for preview badge.
    * */
    showPreviewBadge = (): JSX.Element | undefined => {
        if (this.state.selectedMembers) {
            let recipients: Array<any> = [];
            this.state.selectedMembers.forEach((value) => {
                recipients.push(value.header);
            });

            var badgeAwardPreview: BadgeAwardPreview = {
                AwardedBy: this.state.awardedByName!,
                BadgeName: this.selectedBadge!,
                ImageUrl: this.badgeImage!,
                Narrative: this.state.note!,
                AwardRecipients: recipients,
            };
            return (
                <PreviewBadge BadgeAwardPreview={badgeAwardPreview} ResourceStrings={this.state.resourceStrings} />
            );
        }
    }

    /**
    *  Handles change in note text.
    * */
    onNoteChange(event) {
        this.setState({
            note: event.target.value
        });
    }

    /** 
   *  Returns layout for award badge.
   * */
    showAwardBadge = (): JSX.Element | undefined => {
        const criteriaText = this.badgeCriteria != undefined ? this.badgeCriteria + (this.badgeCriteriaUrl != undefined ? " Url:" + this.badgeCriteriaUrl : '') : "Url: " + this.badgeCriteriaUrl;

        return (
            <div className="container-subdiv" style={{ padding: "1rem" }}>
                <Flex gap="gap.small" hAlign="start" vAlign="center" styles={{ marginTop: "1rem" }}>
                    <Flex.Item align="start" size="size.small" grow>
                        <Text content={this.state.resourceStrings.BadgeToAward} />
                    </Flex.Item>
                </Flex>
                <Flex gap="gap.small" hAlign="start" vAlign="center">
                    <Flex.Item align="start" size="size.small" grow>
                        <Input disabled value={this.selectedBadge!} fluid />
                    </Flex.Item>
                </Flex>

                <Flex gap="gap.small" hAlign="start" vAlign="center" styles={{ marginTop: "1rem" }}>
                    <Flex.Item align="start" size="size.small" grow>
                        <Text content={this.state.resourceStrings.BadgeDescription} />
                    </Flex.Item>
                </Flex>
                <Flex gap="gap.small" hAlign="start" vAlign="center">
                    <Flex.Item align="start" size="size.small" grow>
                        <TextArea title={this.badgeDescription!} styles={{ height: "6rem" }} fluid value={this.badgeDescription!} />
                    </Flex.Item>
                </Flex>
                <Flex gap="gap.small" hAlign="start" vAlign="center" styles={{ marginTop: "1rem" }}>
                    <Flex.Item align="start" size="size.small" grow>
                        <Text content={this.state.resourceStrings.BadgeCriteria} />
                    </Flex.Item>
                </Flex>
                <Flex gap="gap.small" hAlign="start" vAlign="center">
                    <Flex.Item align="start" size="size.small" grow>
                        <TextArea title={criteriaText!} styles={{ height: "6rem" }} fluid value={criteriaText!} />
                    </Flex.Item>
                </Flex>

                <Flex gap="gap.small" hAlign="start" vAlign="center" styles={{ marginTop: "1rem" }}>
                    <Flex.Item align="start" size="size.small" grow>
                        <Text content={this.state.resourceStrings.ToBeAwardedTo} />
                    </Flex.Item>
                </Flex>
                <Flex gap="gap.small" hAlign="start" vAlign="center">
                    <Flex.Item align="start" size="size.small" grow>
                        <Dropdown
                            multiple
                            search
                            fluid
                            items={this.state.allMembers}
                            placeholder={this.state.resourceStrings.SearchTeamMembers}
                            getA11ySelectionMessage={this.onMemberSelectionChanged}
                            noResultsMessage={this.state.resourceStrings.NoMatchesFound}
                            value={this.state.selectedMembers}
                        />
                    </Flex.Item>
                </Flex>
                <Flex gap="gap.small" hAlign="start" vAlign="center" styles={{ marginTop: "1rem" }}>
                    <Flex.Item align="start" size="size.small" grow>
                        <Text content={this.state.resourceStrings.NoteForRecipients} />
                    </Flex.Item>
                </Flex>
                <Flex gap="gap.small" hAlign="start" vAlign="center">
                    <Flex.Item align="start" size="size.small" grow>
                        <TextArea defaultValue="" maxLength={250} styles={{ height: "8rem" }} fluid placeholder={this.state.resourceStrings.NoteForReceipientsPlaceholder} value={this.state.note!} onChange={this.onNoteChange.bind(this)} />
                    </Flex.Item>
                </Flex>
            </div>
        )
    }

    /** Render function. */
    render() {
        /** Return content based on preview button click. */
        const showContent = (): JSX.Element | undefined => {
            if (this.state.isPreviewBadge) {
                return this.showPreviewBadge();
            }
            else {
                return this.showAwardBadge();
            }
        }

        /** Check if resource strings are fetched from API. */
        const renderPageContent = (): JSX.Element => {
            if (this.state.resourceStringsLoaded) {
                return (
                    <Provider theme={this.state.themeStyle}>
                        <div className="container-div">

                                {this.state.isLoading ? <Loader /> : showContent()}
                                <AwardBadgeFooter errorMessage={this.state.errorMessage} isAwardBadgeLoading={this.state.isAwardBadgeLoading} isPreviewBadge={this.state.isPreviewBadge} onBackClick={this.onBackClick} onPreviewClick={this.onPreviewClick} resourceStrings={this.state.resourceStrings} submitAwardBadge={this.submitAwardBadge} />

                        </div>
                    </Provider>
                );
            }
            else {
                return (
                    <Provider theme={this.state.themeStyle}>
                        <Loader />
                    </Provider>
                );
            }
        }

        return (renderPageContent());
    }
}

export default withAITracking(reactPlugin, AwardBadge);