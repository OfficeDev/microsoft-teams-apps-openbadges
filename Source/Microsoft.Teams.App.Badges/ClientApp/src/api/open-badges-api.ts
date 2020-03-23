// <copyright file="open-badges-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { AxiosResponse } from "axios";

const baseAxiosUrl = window.location.origin;

/**
* Get all badges present in issuer group of user.
* @param  {String | Null} token Custom JWT token
*/
export const getAllBadges = async (token: string, userEmail: string): Promise<any> => {

    let url = baseAxiosUrl + "/api/badges/allbadges?email=" + userEmail;
    let allBadgesResponse = await axios.get(url, token);
    if (allBadgesResponse.status === 401) {
        if (allBadgesResponse.data) {
            window.location.href = "/Error?code=" + allBadgesResponse.data.code + "&token=" + token;
        }
        else {
            window.location.href = "/Error?token=" + token;
        }
    }
    else {
        return allBadgesResponse;
    }
}

/**
* Get badges awarded to user.
* @param  {String | Null} token Custom JWT token
*/
export const getMyBadges = async (token: string): Promise<any> => {

    let url = baseAxiosUrl + "/api/badges/EarnedBadges";
    let myBadgesResponse = await axios.get(url, token);
    if (myBadgesResponse.status === 401) {
        redirectToErrorPage(myBadgesResponse, token);
    }
    else {
        return myBadgesResponse;
    }    
}

/**
* Get all team members.
* @param  {String} teamId Team ID for getting members
* @param  {String | Null} token Custom JWT token
*/
export const getMembersInTeam = async (teamId: string, token: string): Promise<any> => {

    let url = baseAxiosUrl + "/api/badges/teammembers?teamId=" + teamId;
    let teamMemberResponse = await axios.get(url, token);
    if (teamMemberResponse.status === 401) {
        redirectToErrorPage(teamMemberResponse, token);
    }
    else {
        return teamMemberResponse;
    }
}

/**
* Get all team members.
* @param  {any | Null} assertionDetails Badge details and members list object
* @param  {String | Null} token Custom JWT token
*/
export const submitAwardBadge = async (assertionDetails: any, token: string): Promise<any> => {

    let url = baseAxiosUrl + "/api/badges/awardbadge";
    let awardBadgeResponse = await axios.post(url, assertionDetails, token);
    if (awardBadgeResponse.status === 401) {
        redirectToErrorPage(awardBadgeResponse, token);
    }
    else {
        return awardBadgeResponse;
    }
}

/**
* Get localized resource strings from API
*/
export const getResourceStrings = async (token: string): Promise<any> => {

    let url = baseAxiosUrl + "/api/resource/resourcestrings";
    let resourceStringsResponse = await axios.get(url, token);
    if (resourceStringsResponse.status === 401) {
        redirectToErrorPage(resourceStringsResponse, token);
    }
    else {
        return resourceStringsResponse;
    }
}

const redirectToErrorPage = (response: AxiosResponse<any>, token: string) => {
    if (response.data) {
        window.location.href = "/Error?code=" + response.data.code + "&token=" + token;
    }
    else {
        window.location.href = "/Error?token=" + token;
    }
}