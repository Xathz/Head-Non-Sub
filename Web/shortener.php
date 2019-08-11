<?php
/*
Nginx or other webservers need to be configured...

    try_files $uri $uri/ /index.php?url=https://hns.xathz.net$request_uri;

*/

// Database connection
define("DB_NAME", "");
define("DB_SERVER", "");
define("DB_USERNAME", "");
define("DB_PASSWORD", "");

// Upload key. Set to something random and long (at least 16).
// The key is required to be able to shorten links.
define("SHORTENER_KEY", "");
setlocale(LC_ALL, "en_US.UTF-8");

 /**
  * Exit the script with a HTTP status code.
  *
  * @param string $responseCode HTTP response status code.
  */
function exitWithStatusCode($responseCode) {
    mysqli_close($dbConnection);
    http_response_code($responseCode);
    exit();
}

 /**
  * Generate a short url.
  *
  * @return string Shortened url.
  */
function generateShortUrl() {
    $shortUrl = "https://hns.xathz.net/" . bin2hex(random_bytes(4));

    while (true) {
        $shortUrlEscaped = mysqli_real_escape_string($dbConnection, $shortUrl);
        $shortUrlQuery = mysqli_query($dbConnection, "SELECT short_url FROM short_urls WHERE short_url = '$shortUrlEscaped';");

        if (mysqli_num_rows($shortUrlQuery) == 0) {
            return $shortUrl;
        } else {
            $shortUrl = "https://hns.xathz.net/" . bin2hex(random_bytes(4));
        }
    }
}

$dbConnection = mysqli_connect(DB_SERVER, DB_USERNAME, DB_PASSWORD, DB_NAME);

if (mysqli_connect_errno()) {
    exitWithStatusCode(503);
}

if (!empty($_POST["key"])) {

    if ($_POST["key"] != SHORTENER_KEY) {
        exitWithStatusCode(401);
    }

    if (!empty($_POST["url"])) {
        $fullUrlEscaped = mysqli_real_escape_string($dbConnection, $_POST["url"]);
        $shortUrlQuery = mysqli_query($dbConnection, "SELECT short_url FROM short_urls WHERE full_url = '$fullUrlEscaped';");

        if (mysqli_num_rows($shortUrlQuery) == 1) {
            $shortUrl = mysqli_fetch_assoc($shortUrlQuery)["short_url"];

            echo $shortUrl;
            exitWithStatusCode(200);
        } else {
            $shortUrl = generateShortUrl();

            $fullUrlEscaped = mysqli_real_escape_string($dbConnection, $_POST["url"]);
            $shortUrlEscaped = mysqli_real_escape_string($dbConnection, $shortUrl);

            if (mysqli_query($dbConnection, "INSERT INTO short_urls (full_url, short_url) VALUES ('$fullUrlEscaped', '$shortUrlEscaped');")) {
                echo $shortUrl;
                exitWithStatusCode(200);
            } else {
                exitWithStatusCode(400);
            }
        }

        exit();
    }

    exitWithStatusCode(400);
} else {

    if (!empty($_GET["url"])) {

        $shortUrlEscaped = mysqli_real_escape_string($dbConnection, $_GET["url"]);
        $fullUrlQuery = mysqli_query($dbConnection, "SELECT full_url FROM short_urls WHERE short_url = '$shortUrlEscaped';");

        if (mysqli_num_rows($fullUrlQuery) == 1) {
            $fullUrl = mysqli_fetch_assoc($fullUrlQuery)["full_url"];
            header("Location: " . $fullUrl, true, 301);
        } else {
            exitWithStatusCode(404);
        }
    
    } else {
        exitWithStatusCode(404);
    }

}

mysqli_close($dbConnection);

?>
