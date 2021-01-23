#!/bin/bash

dotnet build src/OmniSharp.Stdio.Driver

cp -R ./bin/Debug/OmniSharp.Stdio.Driver/net472/* ~/src/omnisharp/omnisharp-server-local/omnisharp/
