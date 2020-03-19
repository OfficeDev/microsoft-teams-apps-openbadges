/*
    <copyright file="index.tsx" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter as Router } from "react-router-dom";
import AppRoute from "./router/router";

ReactDOM.render(
    <Router>
        <AppRoute />
    </Router>, document.getElementById("root"));