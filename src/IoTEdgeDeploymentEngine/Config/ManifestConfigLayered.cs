﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IoTEdgeDeploymentEngine.Config
{
    /// <inheritdoc />
    public class ManifestConfigLayered: IManifestConfig
    {
        /// <inheritdoc />
        public string DirectoryRoot { get; set; }
    }
}