// <copyright file="my-badges.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Text, Flex, Table, Image, Icon } from "@fluentui/react";
import { EarnedBadgeDetails } from "../models/earned-badge-details";
import "../styles/theme.css";

interface IMyBadgesState {
    myBadges: Array<EarnedBadgeDetails>,
    resourceStrings: any
}

/** Component for displaying badges rewarded to a user. */
const MyBadges: React.FunctionComponent<IMyBadgesState> = props => {
    const myBadgesTableHeader = {
        key: "header",
        items: [
            { content: <Text weight="bold" content={props.resourceStrings.Badge} />, key: "badge" },
            { content: <Text weight="bold" content={props.resourceStrings.BadgeName} />, key: "name" },
            { content: <Text weight="bold" content={props.resourceStrings.BadgeDescription} />, key: "description" },
            { content: <Text weight="bold" content={props.resourceStrings.AwardedBy} />, key: "awardedby" },
            { content: <Text weight="bold" content={props.resourceStrings.OnDate} />, key: "date" }
        ],
    };

    if (props.myBadges.length === 0) {
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
                            <Text weight="bold" content={props.resourceStrings.EmptyYourBadgesTitle} /><br />
                            <Text content={props.resourceStrings.EmptyYourBadgesDescription} />
                        </div>
                    </Flex>
                </Flex.Item>
            </Flex>
        )
    }
    else {
        let myEarnedBadges = props.myBadges.map((value: EarnedBadgeDetails, index) => (
            {
                key: index,
                items: [
                    { content: <Image className="badge-icon" src={value.ImageUri} />, key: index + "1" },
                    { content: <Text content={value.Name} />, key: index + "2" },
                    { content: <Text content={value.Description} />, key: index + "3", truncateContent: true },
                    { content: <Text content={value.AwardedBy} />, key: index + "4" },
                    { content: <Text content={value.AwardedOn} />, key: index + "5" },
                ],
            }
        ));
        return (<Table rows={myEarnedBadges} header={myBadgesTableHeader} variables={{ cellContentOverflow: 'none' }} aria-label="Static table" />)
    }
}

export default MyBadges;