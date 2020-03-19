/*
    <copyright file="badge-details.tsx" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

export class BadgeDetails {
    entityType: string | "" = "";
    entityId: string | "" = "";
    openBadgeId: string | "" = "";
    createdAt: Date | null = null;
    createdBy: string | "" = "";
    issuer: string | "" = "";
    issuerOpenBadgeId: string | "" = "";
    name: string | "" = "";
    image: string | "" = "";
    description: string | "" = "";
    criteriaUrl: string | "" = "";
    criteriaNarrative: string | "" = "";
    alignments: Array<any> | null = null;
    tags: Array<string> | null = null;
    expires: Array<any> | null = null;
}