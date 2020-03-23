/*
    <copyright file="router.tsx" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

import * as React from "react";
import { BrowserRouter, Route, Switch } from "react-router-dom";
import Badges from "../components/badges";
import ErrorPage from "../components/error-page";
import AwardBadge from "../components/award-badge";
import PreviewAward from "../components/preview-award";

const AppRoute = () => {
    return (
        <BrowserRouter>
            <Switch>
                <Route exact path="/AllBadges" component={Badges} />
                <Route exact path="/AwardBadge" component={AwardBadge} />
                <Route exact path="/PreviewAward" component={PreviewAward} />
                <Route exact path="/Error" component={ErrorPage} />
            </Switch>
        </BrowserRouter>
    );
}
export default AppRoute;
