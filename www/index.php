<?php
	$xml = simplexml_load_file("trayradio.xml");
    if (isset($_GET["get"]))
    {
        header("location: " . $xml->UpdateLink);
    }
?>
<html>
<head>
    <title><?php echo($xml->Name) ?></title>
    <style>
        a, a:hover {
            color: orange;
            font-weight: bold;
            text-decoration: none;
        }

        a:hover {
            color: darkorange;
        }

        div.cp {
            font-size: 12px;
        }

        body {
            font-family: "Trebuchet MS";
            margin-top: 40px;
            text-align: center;
        }
    </style>
        
    </script>
</head>
    <body>
        <img src="./images/antenna.png" alt="Tray Radio Antenna :)" height="96px" width="96px" style="margin-bottom: 20px;">
        <h1><?php echo($xml->Name . " " . $xml->Version) ?></h1>
        <div style="margin-bottom: 20px;">Relesed <?php echo($xml->ReleaseDate) ?></div>
        <div style="margin-bottom: 20px;"><a href="?get">Download</a></div>
        <div>Simple internet radio player for Windows inspired by Linux <a href="http://radiotray.sourceforge.net/">RadioTray</a>.</div>
        <div style="margin: 0 auto; margin-top: 20px; margin-bottom: 20px; display: inline-block;">
            <table>
                <tr>
                    <?php
                        if ($handle = opendir('./images/')) {
                            while (false !== ($entry = readdir($handle))) {
                                if ($entry != "." && $entry != ".." && $entry != "antenna.png") {
                                    echo("<td><a href=\"./images/$entry\"><img height=\"40px\" width=\"100px\" src=\"./images/$entry\" alt=$entry></a></td>");
                                }
                            }
                            closedir($handle);
                        }
                    ?>
                </tr>
            </table>
        </div>
        <div style="margin-bottom: 30px;">Changelog and download of the previous versions is in progress.</div>
        <div class="cp">Copyright (c) 2017 Michal Heczko</div>
        <div class="cp">Michal Heczko &lt<a href="mailto:micky.heczko@gmail.com">micky.heczko@gmail.com</a>&gt</div>
    </body>
</html>
