// <copyright file="all-badges.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Text, Flex, Grid, Image, Icon } from "@fluentui/react";
import { Link } from "react-router-dom";
import { BadgeAward } from "../models/badge-award";
import { ConfigurationDetails } from "../models/configuration-details";
import { BadgeDetails } from "../models/badge-details";
import "../styles/theme.css";

interface IAllBadgesProps {
    allBadges: Array<BadgeDetails>,
    resourceStrings: any,
    onCreateBadgeClick: (event: any) => void,
    showCreateBadge: boolean,
    configurationDetails: ConfigurationDetails,
    userEmail: string | null;
    backgroundClassName: string;
}

/** Component for displaying all badges which can be awarded to user. */
const AllBadges: React.FunctionComponent<IAllBadgesProps> = props => {
    let badges: Array<any> = [];

    const getBadgePageDetails = (badge) => {
        const badgeAward: BadgeAward = {
            BadgeName: badge.name,
            BadgeId: badge.entityId,
            ImageUrl: badge.image,
            AwardedBy: props.userEmail,
            BadgeDescription: badge.description,
            Criteria: badge.criteriaNarrative,
            CriteriaUrl: badge.criteriaUrl
        }
        return {
            BadgeAward: badgeAward,
            ConfigurationDetails: props.configurationDetails
        }
    }

    const onCreateBadgeClick = (event: any) => {
        props.onCreateBadgeClick(event);
    }

    if (props.showCreateBadge) {
        badges.push(
            <Flex column gap="gap.smaller" vAlign="center" className={props.backgroundClassName} styles={{ padding: "1rem" }} onClick={onCreateBadgeClick} >
                <Flex hAlign="center">
                    <Icon name="add" size="largest" circular bordered />
                </Flex>
                <Flex hAlign="center">
                    <Text weight="bold" align="center" content={props.resourceStrings.CreateBadgeName} />
                </Flex>
            </Flex>
        );
    }
    else {
        if (props.allBadges.length === 0) {
            return (
                <Flex gap="gap.small">
                    <Flex.Item>
                        <div
                            style={{
                                position: "relative",
                            }}
                        >
                            <Icon outline color="green" name="question-circle" />
                        </div>
                    </Flex.Item>

                    <Flex.Item grow>
                        <Flex column gap="gap.small" vAlign="stretch">
                            <div>
                                <Text weight="bold" content={props.resourceStrings.EmptyAllBadgesTitle} /><br />
                                <Text content={props.resourceStrings.EmptyAllBadgesDescription} />
                            </div>
                        </Flex>
                    </Flex.Item>
                </Flex>
            )
        }
    }

    badges.push(props.allBadges.map((value: BadgeDetails, index) => (
        <Link to={{
            pathname: "/AwardBadge", state:
                getBadgePageDetails(value)

        }} style={{ textDecoration: "none", color: "inherit" }}>
            <Flex column gap="gap.smaller" vAlign="center" className={props.backgroundClassName} styles={{ padding: "1rem" }} >
                <Flex hAlign="center">
                    <Image key={"i" + index.toString()} fluid src={value.image} styles={{ height: "7rem", width: "7rem" }} />
                </Flex>
                <Flex hAlign="center">
                    <Text weight="bold" align="center" content={value.name} />
                </Flex>
            </Flex>
        </Link>
    )));
    return (<Grid columns="3" content={badges} />);
}

export default AllBadges;