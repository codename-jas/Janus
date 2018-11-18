DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
FILE_NAME="$DIR/../codecov.xml"
curl -s https://codecov.io/bash > codecov
chmod +x codecov
./codecov -f "$FILE_NAME"
