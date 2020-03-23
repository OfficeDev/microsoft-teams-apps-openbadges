/*
    <copyright file="preview-award.tsx" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

import * as React from "react";
import { Text, Flex, Image, Header } from "@fluentui/react";
import { BadgeAwardPreview } from "../models/badge-award-preview";
import "../styles/theme.css";

interface IBadgesProps {
    BadgeAwardPreview: BadgeAwardPreview,
    ResourceStrings: any
};


/** Component for previewing badge created before sharing in team. */
const PreviewAward = (props: IBadgesProps): JSX.Element => {

    /**
    *  Returns the badge preview to parent.
    * */
    return (
        <>
            <Flex gap="gap.medium" padding="padding.medium" hAlign="start" vAlign="center">
                <Flex.Item align="start" size="size.small" grow>
                    <Flex column gap="gap.small" vAlign="stretch">
                        <Flex space="between">
                            <Text content={props.ResourceStrings.PreviewBadgeTitle} />
                        </Flex>
                    </Flex>
                </Flex.Item>
            </Flex>
            <div className="div-shadow" style={{ padding: "5px 10px" }}>
                <Flex gap="gap.medium" padding="padding.medium" hAlign="start" vAlign="center">
                    <Flex.Item align="start" size="size.small" grow>
                        <Flex column gap="gap.small" vAlign="stretch">
                            <Flex space="between">
                                <Header as="h2" content={props.BadgeAwardPreview.BadgeName} />
                            </Flex>
                        </Flex>
                    </Flex.Item>
                    <Flex.Item align="start" size="size.small">
                        <div
                            style={{
                                position: "relative",
                                maxWidth: "90px"
                            }}
                        >
                            <Image fluid src={props.BadgeAwardPreview.ImageUrl} />
                        </div>
                    </Flex.Item>
                </Flex>

                <Flex gap="gap.medium" padding="padding.medium" hAlign="start" vAlign="center">
                    <Flex.Item align="start" size="size.small" grow>
                        <Flex column gap="gap.small" vAlign="stretch">
                            <Flex space="between">
                                <Text content={props.ResourceStrings.AwardedTo.replace("{0}", props.BadgeAwardPreview.AwardedBy)} />
                            </Flex>
                        </Flex>
                    </Flex.Item>
                </Flex>
                <Flex gap="gap.medium" padding="padding.medium" hAlign="start" vAlign="center">
                    <Flex.Item align="start" size="size.small" grow>
                        <Flex column gap="gap.small" vAlign="stretch">
                            <Flex space="between">
                                <Text weight="bold" content={props.BadgeAwardPreview.AwardRecipients.join(", ")} />
                            </Flex>
                        </Flex>
                    </Flex.Item>
                </Flex>

                <Flex gap="gap.medium" padding="padding.medium" hAlign="start" vAlign="center" styles={{ wordBreak: "break-word" }}>
                    <Flex.Item align="start" size="size.small" grow>
                        <Flex column gap="gap.small" vAlign="stretch">
                            <Flex space="between">
                                <Text styles={{ overflow: "hidden" }} content={props.BadgeAwardPreview.Narrative} />
                            </Flex>
                        </Flex>
                    </Flex.Item>
                </Flex>
            </div>
        </>
    );
};

export default PreviewAward;