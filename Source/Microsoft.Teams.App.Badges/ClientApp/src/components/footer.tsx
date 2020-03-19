// <copyright file="footer.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Text, Flex, Button } from "@fluentui/react";
import "../styles/theme.css";

interface IAwardBadgeFooterProps {
    resourceStrings: any,
    onBackClick: (event: any) => void,
    onPreviewClick: (event: any) => void,
    submitAwardBadge: (event: any) => void,
    isPreviewBadge: boolean,
    errorMessage: string | null,
    isAwardBadgeLoading: boolean
}

const AwardBadgeFooter: React.FunctionComponent<IAwardBadgeFooterProps> = props => {

    const onBackClick = (event: any): void => {
        props.onBackClick(event);
    }
    const onPreviewClick = (event: any): void => {
        props.onPreviewClick(event);
    }
    const submitAwardBadge = (event: any): void => {
        props.submitAwardBadge(event);
    }

    return (
        <div className="footer">
            <Flex gap="gap.small">
                {props.isPreviewBadge === false && props.errorMessage !== null && <Text styles={{ marginLeft:"1rem" }} content={props.errorMessage} error />}
            </Flex>
            <Flex gap="gap.small">
                <Button icon="icon-chevron-start" text content="Back" onClick={onBackClick} styles={{ marginLeft:"-1rem" }} />
                <Flex.Item push>
                    {props.isPreviewBadge === false ? <Button content={props.resourceStrings.Preview} onClick={onPreviewClick} /> : <span></span>}
                </Flex.Item>
                <Button loading={props.isAwardBadgeLoading} disabled={props.isAwardBadgeLoading} primary content={props.resourceStrings.Award} onClick={submitAwardBadge} />
            </Flex>
        </div>
    );
}

export default AwardBadgeFooter;