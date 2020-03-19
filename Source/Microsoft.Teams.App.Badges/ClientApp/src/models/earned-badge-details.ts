/*
    <copyright file="earned-badge-details.tsx" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

export class EarnedBadgeDetails {
    Name: string | "" = "";
    ImageUri: string | "" = "";
    Description: string | "" = "";
    AwardedOn: Date | null = null;
    AwardedBy: string | "" = "";
}