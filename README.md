# TagDetection

This is a simple reader program that enables a commercial off-the-shelf (**COTS**) reader (Impinj SpeedWay R220, R420, etc.) to get **multiple** tags' info (Received Signal Strength (RSS), Phase, Doppler Shift, etc.) with **multiple antennas** (up to 4) in real time.

The results are logged into a **csv** file, you can personalize the reader parameters by changing the **source code**.

It is under developing, any questions or suggestions is very appreciated.


# DataAnalysis
This project was used to localize the position of antenna according its accumulated phases. For more details, please read the following publications.
* Wang J, Adib F, Knepper R, et al. **Rf-compass: robot object manipulation using rfids**[C]//Proceedings of the 19th annual international conference on Mobile computing & networking. ACM, 2013: 3-14. http://people.csail.mit.edu/rak/www/sites/default/files/pubs/WanEtal13.pdf
* Liu T, Yang L, Lin Q, et al. **Anchor-free backscatter positioning for rfid tags with high accuracy**[C]//INFOCOM, 2014 Proceedings IEEE. IEEE, 2014: 379-387. http://netlab.csu.edu.cn/infocom2014/papers/1569805925.pdf
* Han J, Qian C, Wang X, et al. **Twins: Device-free object tracking using passive tags**[C]//INFOCOM, 2014 Proceedings IEEE. IEEE, 2014: 469-476. http://netlab.csu.edu.cn/infocom2014/papers/1569799611.pdf


Result:
![](https://github.com/MarinYoung4596/RFID-READER/blob/master/DataAnalysis/MATLAB/COTS/sketch.png)