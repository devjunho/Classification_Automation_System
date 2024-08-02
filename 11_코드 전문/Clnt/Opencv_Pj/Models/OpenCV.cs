using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace Opencv_Pj.Models
{
    public class OpenCV
    {
        private CascadeClassifier cascadeClassifier;

        public OpenCV()
        {
            cascadeClassifier = new CascadeClassifier(@"C:\Users\aiot\source\repos\Opencv_Pj\haarcascade_frontalface_alt.xml");
        }

        public Mat FaceDetection(Mat src)
        {
            Mat result = src.Clone();
            using (var gray = new Mat())
            {
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.EqualizeHist(gray, gray);

                Rect[] faces = cascadeClassifier.DetectMultiScale(
                    gray,
                    scaleFactor: 1.1,
                    minNeighbors: 3,
                    flags: HaarDetectionTypes.DoRoughSearch | HaarDetectionTypes.ScaleImage,
                    minSize: new Size(30, 30)
                );

                foreach (Rect face in faces)
                {
                    Cv2.Rectangle(result, face, Scalar.Red, 2);
                }
            }
            return result;
        }

        internal Rect[] GetDetectedFaces(Mat gray)
        {
            throw new NotImplementedException();
        }

        public Rect[] DetectFaces(Mat src)
        {
            using (var gray = new Mat())
            {
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.EqualizeHist(gray, gray);

                Rect[] faces = cascadeClassifier.DetectMultiScale(
                    gray,
                    scaleFactor: 1.1,
                    minNeighbors: 3,
                    flags: HaarDetectionTypes.DoRoughSearch | HaarDetectionTypes.ScaleImage,
                    minSize: new Size(30, 30)
                );

                return faces;
            }
        }
    }
}
