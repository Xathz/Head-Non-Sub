<?php
/*
Nginx or other webservers need to be configured to serve certain files.
Text files need to be UTF-8.

    rewrite "^/(.*).(txt|png|jpe?g|gif)" /uploads/$1.$2;

    location "/uploads/" {
        location ~* "\.(txt)$" {
            allow all;
            add_header Content-Type "text/plain; charset=UTF-8";
        }

        location ~* "\.(png)$" {
            allow all;
            add_header Content-Type "image/png";
        }

        location ~* "\.(jpe?g)$" {
            allow all;
            add_header Content-Type "image/jpeg";
        }

        location ~* "\.(gif)$" {
            allow all;
            add_header Content-Type "image/gif";
        }
    }

*/

// Upload key. Set to something random and long (at least 16).
// The key is required to be able to upload files.
define("UPLOAD_KEY", "");
define("UPLOAD_DIRECTORY", "uploads/");
setlocale(LC_ALL, "en_US.UTF-8");

 /**
  * Generate a file path and name.
  *
  * @param string $fileName Name of the original file to get the extension from.
  * @return array (File path, File name).
  */
function generateFilePath($fileName) {
    $uploadedExtension = pathinfo($fileName, PATHINFO_EXTENSION);

    $path = UPLOAD_DIRECTORY . bin2hex(random_bytes(8)) . "." . $uploadedExtension;

    while (true) {
        if (!file_exists($path)) { break; }

        $path = UPLOAD_DIRECTORY . bin2hex(random_bytes(8)) . "." . $uploadedExtension;
    }

    return array($path, str_replace(UPLOAD_DIRECTORY, "", $path));
}

 /**
  * Validate if a file or url is an allowed type
  *
  * @param string $fileName Name of the file or url to validate.
  */
function validateFiletype($fileName) {
    $uploadedExtension = pathinfo($fileName, PATHINFO_EXTENSION);

    if (!in_array($uploadedExtension, array("txt", "png", "jpg", "jpeg", "gif"))) {
        http_response_code(415);
        exit();
    }
}

if (!empty($_POST["key"])) {

    if ($_POST["key"] != UPLOAD_KEY) {
        http_response_code(401);
        exit();
    }

    if (!empty($_FILES["file"])) {
        validateFiletype($_FILES["file"]["name"]);

        list($filePath, $fileName) = generateFilePath($_FILES["file"]["name"]);

        if (move_uploaded_file($_FILES["file"]["tmp_name"], $filePath)) {
            echo $fileName;
            http_response_code(200);
        } else {
            http_response_code(404);
        }

        exit();
    }

    if (!empty($_POST["string"])) {
        list($filePath, $fileName) = generateFilePath("uploaded_string.txt");

        file_put_contents($filePath, $_POST["string"]);

        if (file_exists($filePath)) { 
            echo $fileName;
            http_response_code(200);
         } else {
            http_response_code(404);
         }

         exit();
    }

    if (!empty($_POST["url"])) {
        validateFiletype(parse_url($_POST["url"], PHP_URL_PATH));

        list($filePath, $fileName) = generateFilePath(parse_url($_POST["url"], PHP_URL_PATH));

        file_put_contents($filePath, fopen($_POST["url"], 'r'));

        if (file_exists($filePath)) { 
            echo $fileName;
            http_response_code(200);
         } else {
            http_response_code(404);
         }

         exit();
    }

    http_response_code(400);
} else {
    http_response_code(401);
}

?>
