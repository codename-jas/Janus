DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
SLN="$DIR/../src/Janus.sln"
echo "Running build in $DIR for $SLN"
dotnet clean $SLN
dotnet restore $SLN
dotnet build $SLN
