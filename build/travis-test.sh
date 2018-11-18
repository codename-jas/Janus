DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
JANUS_CORE="$DIR/../test/Janus.Core.Test/Janus.Core.Test.csproj"
CODE_COV_FILE="$DIR/../codecov.xml"
dotnet test $JANUS_CORE /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="$CODE_COV_FILE"
