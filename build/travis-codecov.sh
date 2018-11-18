DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
FILE_NAME="$DIR/../codecov.xml"
curl -s https://codecov.io/bash > codecov
./codecov -f "$FILE_NAME" -t "12073bb6-dd4d-4a10-9731-6b7eaa7bf559"
