///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Class:              NII2Volume, NIfTI
/// Description:        Loading NIfTI files into a volume object.
/// Author:             LISCINTEC
///                         http://www.liscintec.com
///                         info@liscintec.com
/// Date:               Feb 2017
/// Notes:              -
/// Revision History:   First release
/// 
/// This file is part of the Volume Viewer Pro Package.
/// Volume Viewer Pro is a Unity Asset Store product.
/// https://www.assetstore.unity3d.com/#!/content/83185
///-----------------------------------------------------------------

#if !NETFX_CORE
using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;

namespace VolumeViewer
{
    public struct NIfTI
    {
        public const int LSB_FIRST = 1;
        public const int MSB_FIRST = 2;

        public enum FormatCode
        {
            NIFTI_TYPE_UNKNOWN = 0,
            NIFTI_TYPE_BINARY = 1,
            NIFTI_TYPE_UINT8 = 2,
            NIFTI_TYPE_INT16 = 4,
            NIFTI_TYPE_INT32 = 8,
            NIFTI_TYPE_FLOAT32 = 16,
            NIFTI_TYPE_COMPLEX64 = 32,
            NIFTI_TYPE_FLOAT64 = 64,
            NIFTI_TYPE_RGB24 = 128,
            NIFTI_TYPE_INT8 = 256,
            NIFTI_TYPE_UINT16 = 512,
            NIFTI_TYPE_UINT32 = 768,
            NIFTI_TYPE_INT64 = 1024,
            NIFTI_TYPE_UINT64 = 1280,
            NIFTI_TYPE_FLOAT128 = 1536,
            NIFTI_TYPE_COMPLEX128 = 1792,
            NIFTI_TYPE_COMPLEX256 = 2048,
            NIFTI_TYPE_RGBA32 = 2304
        }
        
        public enum IntentCode
        {
            NIFTI_INTENT_NONE = 0,
            NIFTI_INTENT_CORREL = 2,
            NIFTI_INTENT_TTEST = 3,
            NIFTI_INTENT_FTEST = 4,
            NIFTI_INTENT_ZSCORE = 5,
            NIFTI_INTENT_CHISQ = 6,
            NIFTI_INTENT_BETA = 7,
            NIFTI_INTENT_BINOM = 8,
            NIFTI_INTENT_GAMMA = 9,
            NIFTI_INTENT_POISSON = 10,
            NIFTI_INTENT_NORMAL = 11,
            NIFTI_INTENT_FTEST_NONC = 12,
            NIFTI_INTENT_CHISQ_NONC = 13,
            NIFTI_INTENT_LOGISTIC = 14,
            NIFTI_INTENT_LAPLACE = 15,
            NIFTI_INTENT_UNIFORM = 16,
            NIFTI_INTENT_TTEST_NONC = 17,
            NIFTI_INTENT_WEIBULL = 18,
            NIFTI_INTENT_CHI = 19,
            NIFTI_INTENT_INVGAUSS = 20,
            NIFTI_INTENT_EXTVAL = 21,
            NIFTI_INTENT_PVAL = 22,
            NIFTI_INTENT_LOGPVAL = 23,
            NIFTI_INTENT_LOG10PVAL = 24,
            NIFTI_FIRST_STATCODE = 2,
            NIFTI_LAST_STATCODE = 24,
            NIFTI_INTENT_ESTIMATE = 1001,
            NIFTI_INTENT_LABEL = 1002,
            NIFTI_INTENT_NEURONAME = 1003,
            NIFTI_INTENT_GENMATRIX = 1004,
            NIFTI_INTENT_SYMMATRIX = 1005,
            NIFTI_INTENT_DISPVECT = 1006,
            NIFTI_INTENT_VECTOR = 1007,
            NIFTI_INTENT_POINTSET = 1008,
            NIFTI_INTENT_TRIANGLE = 1009,
            NIFTI_INTENT_QUATERNION = 1010,
            NIFTI_INTENT_DIMLESS = 1011,
            NIFTI_INTENT_TIME_SERIES = 2001,
            NIFTI_INTENT_NODE_INDEX = 2002,
            NIFTI_INTENT_RGB_VECTOR = 2003,
            NIFTI_INTENT_RGBA_VECTOR = 2004,
            NIFTI_INTENT_SHAPE = 2005
        }

        public enum FormCode
        {
            NIFTI_XFORM_UNKNOWN = 0,
            NIFTI_XFORM_SCANNER_ANAT = 1,
            NIFTI_XFORM_ALIGNED_ANAT = 2,
            NIFTI_XFORM_TALAIRACH = 3,
            NIFTI_XFORM_MNI_152 = 4
        }

        public enum UnitCode
        {
            NIFTI_UNITS_UNKNOWN = 0,
            NIFTI_UNITS_METER = 1,
            NIFTI_UNITS_MM = 2,
            NIFTI_UNITS_MICRON = 3,
            NIFTI_UNITS_SEC = 8,
            NIFTI_UNITS_MSEC = 16,
            NIFTI_UNITS_USEC = 24,
            NIFTI_UNITS_HZ = 32,
            NIFTI_UNITS_PPM = 40,
            NIFTI_UNITS_RADS = 48
        }

        public enum SliceOrderCode
        {
            NIFTI_SLICE_UNKNOWN = 0,
            NIFTI_SLICE_SEQ_INC = 1,
            NIFTI_SLICE_SEQ_DEC = 2,
            NIFTI_SLICE_ALT_INC = 3,
            NIFTI_SLICE_ALT_DEC = 4,
            NIFTI_SLICE_ALT_INC2 = 5,
            NIFTI_SLICE_ALT_DEC2 = 6
        }

        public enum OrientationCode
        {
            NIFTI_L2R = 1,
            NIFTI_R2L = 2,
            NIFTI_P2A = 3,
            NIFTI_A2P = 4,
            NIFTI_I2S = 5,
            NIFTI_S2I = 6
        }

        public enum ExtensionCode
        {
            NIFTI_ECODE_IGNORE = 0,
            NIFTI_ECODE_DICOM = 2,
            NIFTI_ECODE_AFNI = 4,
            NIFTI_ECODE_COMMENT = 6,
            NIFTI_ECODE_XCEDE = 8,
            NIFTI_ECODE_JIMDIMINFO = 10,
            NIFTI_ECODE_WORKFLOW_FWDS = 12,
            NIFTI_ECODE_FREESURFER = 14,
            NIFTI_ECODE_PYPICKLE = 16,
            NIFTI_ECODE_MIND_IDENT = 18,
            NIFTI_ECODE_B_VALUE = 20,
            NIFTI_ECODE_SPHERICAL_DIRECTION = 22,
            NIFTI_ECODE_DT_COMPONENT = 24,
            NIFTI_ECODE_SHC_DEGREEORDER = 26,
            NIFTI_ECODE_VOXBO = 28,
            NIFTI_ECODE_CARET = 30,
            NIFTI_MAX_ECODE = 30
        }

        public enum FileCode
        {
            NIFTI_FTYPE_ANALYZE = 0,
            NIFTI_FTYPE_NIFTI1_1 = 1,
            NIFTI_FTYPE_NIFTI1_2 = 2,
            NIFTI_FTYPE_ASCII = 3,
            NIFTI_MAX_FTYPE = 3
        }

        public enum anal75OrientCode
        {
            a75_transverse_unflipped = 0,
            a75_coronal_unflipped = 1,
            a75_sagittal_unflipped = 2,
            a75_transverse_flipped = 3,
            a75_coronal_flipped = 4,
            a75_sagittal_flipped = 5,
            a75_orient_unknown = 6
        }

        public static byte XYZT_TO_SPACE(byte xyzt) { return (byte)(xyzt & 0x07); }
        public static byte XYZT_TO_TIME(byte xyzt) { return (byte)(xyzt & 0x38); }
        public static byte SPACE_TIME_TO_XYZT(byte ss, byte tt) { return (byte)((((byte)(ss)) & 0x07) | (((byte)(tt)) & 0x38)); }
        public static byte DIM_INFO_TO_FREQ_DIM(byte di) { return (byte)(((di)) & 0x03); }
        public static byte DIM_INFO_TO_PHASE_DIM(byte di) { return (byte)(((di) >> 2) & 0x03); }
        public static byte DIM_INFO_TO_SLICE_DIM(byte di) { return (byte)(((di) >> 4) & 0x03); }
        public static byte FPS_INTO_DIM_INFO(byte fd, byte pd, byte sd) { return (byte)(((((byte)(fd)) & 0x03)) | ((((byte)(pd)) & 0x03) << 2) | ((((byte)(sd)) & 0x03) << 4)); }

        public static int NIFTI_VERSION(Nii1Header h) { return (((h).magic[0] == 'n' && (h).magic[3] == '\0' && ((h).magic[1] == 'i' || (h).magic[1] == '+') && ((h).magic[2] >= '1' && (h).magic[2] <= '9')) ? (h).magic[2] - '0' : 0); }
        public static bool NIFTI_ONEFILE(Nii1Header h) { return ((h).magic[1] == '+'); }
        public static bool NIFTI_NEEDS_SWAP(Nii1Header h) { return ((h).dim[0] < 0 || (h).dim[0] > 7); }
        public static int NIFTI_5TH_DIM(Nii1Header h) { return (((h).dim[0] > (short)4 && (h).dim[5] > (short)1) ? (h).dim[5] : (short)0); }
        public static float FIXED_FLOAT(float f) { return (float.IsInfinity(f) || float.IsNaN(f)) ? 0.0f : f; }

        public static bool isNIFTI(string fName)
        {
            string hdrName = null;
            string fExt = fName.Substring(fName.Length - 4, 4);
            string fBase = fName.Substring(0, fName.Length - 4);
            if (fExt.Equals(".nii", StringComparison.OrdinalIgnoreCase) && File.Exists(fBase + ".nii"))
            {
                hdrName = fName;
            }
            else if ((fExt.Equals(".hdr", StringComparison.OrdinalIgnoreCase) || fExt.Equals(".img", StringComparison.OrdinalIgnoreCase)) && File.Exists(fBase + ".hdr") && File.Exists(fBase + ".img"))
            {
                hdrName = fBase + ".hdr";
            }
            if (hdrName == null || !File.Exists(hdrName))
            {
                return false;
            }
            Int64 fileSize = new System.IO.FileInfo(hdrName).Length;
            if (fileSize < 348)
            {
                return false;
            }
            int sizeof_hdr = 0;
            byte[] magic = null;
            BinaryReader reader;
            try
            {
                reader = new BinaryReader(File.Open(fName, FileMode.Open));
                if (reader != null)
                {
                    sizeof_hdr = reader.ReadInt32();
                    reader.BaseStream.Seek(344, SeekOrigin.Begin);
                    magic = reader.ReadBytes(4);
                    reader.BaseStream.Close();
                    reader.Close();
                }
            }
            catch (IOException)                 {   Debug.Log("IOException: " + fName);                     return false; }
            catch (UnauthorizedAccessException) {   Debug.Log("UnauthorizedAccessException: " + fName);     return false; }
            catch (NotSupportedException)       {   Debug.Log("NotSupportedException: " + fName);           return false; }
            catch (ArgumentException)           {   Debug.Log("ArgumentException: " + fName);               return false; }
            if (sizeof_hdr == 348 || BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(sizeof_hdr)), 0) == 348)
            {
                return true;
            }
            if (magic[0] == 'n' && (magic[1] == 'i' || magic[1] == '+') && magic[2] == '1' && magic[3] == '\0')
            {
                return true;
            }
            return false;
        }

        public static byte[] swapBytes(int numElements, int numBytes, byte[] array)
        {
            byte temp;
            for (int i = 0; i < numElements; i++)
            {
                for (int j = 0; j < numBytes; j += 2)
                {
                    temp = array[i * numBytes + j];
                    array[i * numBytes + j] = array[(i + 1) * numBytes - j - 1];
                    array[(i + 1) * numBytes - j - 1] = temp;
                }
            }
            return array;
        }

        public static void getFormatSize(FormatCode format, out int bytesPerVoxel, out int bytesPerChannel, out int numChannels)
        {
            bytesPerVoxel = 0;
            bytesPerChannel = 0;
            numChannels = 0;
            switch (format)
            {
                case FormatCode.NIFTI_TYPE_INT8:
                case FormatCode.NIFTI_TYPE_UINT8:
                    bytesPerVoxel = 1;
                    bytesPerChannel = 1;
                    numChannels = 1;
                    break;
                case FormatCode.NIFTI_TYPE_INT16:
                case FormatCode.NIFTI_TYPE_UINT16:
                    bytesPerVoxel = 2;
                    bytesPerChannel = 2;
                    numChannels = 1;
                    break;
                case FormatCode.NIFTI_TYPE_RGB24:
                    bytesPerVoxel = 3;
                    bytesPerChannel = 1;
                    numChannels = 3;
                    break;
                case FormatCode.NIFTI_TYPE_RGBA32:
                    bytesPerVoxel = 4;
                    bytesPerChannel = 1;
                    numChannels = 4;
                    break;
                case FormatCode.NIFTI_TYPE_INT32:
                case FormatCode.NIFTI_TYPE_UINT32:
                case FormatCode.NIFTI_TYPE_FLOAT32:
                    bytesPerVoxel = 4;
                    bytesPerChannel = 4;
                    numChannels = 1;
                    break;
                case FormatCode.NIFTI_TYPE_COMPLEX64:
                    bytesPerVoxel = 8;
                    bytesPerChannel = 4;
                    numChannels = 2;
                    break;
                case FormatCode.NIFTI_TYPE_FLOAT64:
                case FormatCode.NIFTI_TYPE_INT64:
                case FormatCode.NIFTI_TYPE_UINT64:
                    bytesPerVoxel = 8;
                    bytesPerChannel = 8;
                    numChannels = 1;
                    break;
                case FormatCode.NIFTI_TYPE_FLOAT128:
                    bytesPerVoxel = 16;
                    bytesPerChannel = 16;
                    numChannels = 1;
                    break;
                case FormatCode.NIFTI_TYPE_COMPLEX128:
                    bytesPerVoxel = 16;
                    bytesPerChannel = 8;
                    numChannels = 2;
                    break;
                case FormatCode.NIFTI_TYPE_COMPLEX256:
                    bytesPerVoxel = 32;
                    bytesPerChannel = 16;
                    numChannels = 2;
                    break;
            }
        }

        public static void swapNii1Header(ref Nii1Header header)
        {
            header.sizeof_hdr = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.sizeof_hdr)), 0);
            header.extents = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.extents)), 0);
            header.session_error = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.session_error)), 0);
            byte[] temp = new byte[2 * 8];
            Buffer.BlockCopy(header.dim, 0, temp, 0, 2 * 8);
            Buffer.BlockCopy(swapBytes(8, 2, temp), 0, header.dim, 0, 2 * 8);
            header.intent_p1 = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.intent_p1)), 0);
            header.intent_p2 = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.intent_p2)), 0);
            header.intent_p3 = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.intent_p3)), 0);
            header.intent_code = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.intent_code)), 0);
            header.datatype = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.datatype)), 0);
            header.bitpix = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.bitpix)), 0);
            header.slice_start = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.slice_start)), 0);
            temp = new byte[4 * 8];
            Buffer.BlockCopy(header.pixdim, 0, temp, 0, 4 * 8);
            Buffer.BlockCopy(swapBytes(8, 4, temp), 0, header.pixdim, 0, 4 * 8);
            header.vox_offset = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.vox_offset)), 0);
            header.scl_slope = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.scl_slope)), 0);
            header.scl_inter = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.scl_inter)), 0);
            header.slice_end = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.slice_end)), 0);
            header.cal_max = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.cal_max)), 0);
            header.cal_min = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.cal_min)), 0);
            header.slice_duration = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.slice_duration)), 0);
            header.toffset = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.toffset)), 0);
            header.glmax = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.glmax)), 0);
            header.glmin = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.glmin)), 0);
            header.qform_code = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.qform_code)), 0);
            header.sform_code = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.sform_code)), 0);
            header.quatern_b = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.quatern_b)), 0);
            header.quatern_c = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.quatern_c)), 0);
            header.quatern_d = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.quatern_d)), 0);
            header.qoffset_x = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.qoffset_x)), 0);
            header.qoffset_y = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.qoffset_y)), 0);
            header.qoffset_z = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.qoffset_z)), 0);
            temp = new byte[4 * 4];
            Buffer.BlockCopy(header.srow_x, 0, temp, 0, 4 * 4);
            Buffer.BlockCopy(swapBytes(4, 4, temp), 0, header.srow_x, 0, 4 * 4);
            Buffer.BlockCopy(header.srow_y, 0, temp, 0, 4 * 4);
            Buffer.BlockCopy(swapBytes(4, 4, temp), 0, header.srow_y, 0, 4 * 4);
            Buffer.BlockCopy(header.srow_z, 0, temp, 0, 4 * 4);
            Buffer.BlockCopy(swapBytes(4, 4, temp), 0, header.srow_z, 0, 4 * 4);
        }

        public static void swapAnal75Header(ref Anal75Header header)
        {
            header.sizeof_hdr = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.sizeof_hdr)), 0);
            header.extents = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.extents)), 0);
            header.session_error = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.session_error)), 0);
            byte[] temp = new byte[2 * 8];
            Buffer.BlockCopy(header.dim, 0, temp, 0, 2 * 8);
            Buffer.BlockCopy(swapBytes(8, 2, temp), 0, header.dim, 0, 2 * 8);
            header.unused8 = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.unused8)), 0);
            header.unused9 = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.unused9)), 0);
            header.unused10 = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.unused10)), 0);
            header.unused11 = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.unused11)), 0);
            header.unused12 = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.unused12)), 0);
            header.unused13 = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.unused13)), 0);
            header.unused14 = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.unused14)), 0);
            header.datatype = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.datatype)), 0);
            header.bitpix = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.bitpix)), 0);
            header.dim_un0 = BitConverter.ToInt16(swapBytes(1, 2, BitConverter.GetBytes(header.dim_un0)), 0);
            temp = new byte[4 * 8];
            Buffer.BlockCopy(header.pixdim, 0, temp, 0, 4 * 8);
            Buffer.BlockCopy(swapBytes(8, 4, temp), 0, header.pixdim, 0, 4 * 8);
            header.vox_offset = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.vox_offset)), 0);
            header.funused1 = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.funused1)), 0);
            header.funused2 = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.funused2)), 0);
            header.funused3 = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.funused3)), 0);
            header.cal_max = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.cal_max)), 0);
            header.cal_min = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.cal_min)), 0);
            header.compressed = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.compressed)), 0);
            header.verified = BitConverter.ToSingle(swapBytes(1, 4, BitConverter.GetBytes(header.verified)), 0);
            header.glmax = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.glmax)), 0);
            header.glmin = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.glmin)), 0);
            header.views = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.views)), 0);
            header.vols_added = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.vols_added)), 0);
            header.start_field = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.start_field)), 0);
            header.field_skip = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.field_skip)), 0);
            header.omax = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.omax)), 0);
            header.omin = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.omin)), 0);
            header.smax = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.smax)), 0);
            header.smin = BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.smin)), 0);
        }

        public static int getByteOrder()
        {
            return BitConverter.IsLittleEndian ? LSB_FIRST : MSB_FIRST;
        }

        public static int getHeaderType(Nii1Header header)
        {
            if (NIFTI_VERSION(header) != 0)
            {
                return 1;
            }
            if (header.sizeof_hdr == 348 || BitConverter.ToInt32(swapBytes(1, 4, BitConverter.GetBytes(header.sizeof_hdr)), 0) == 348)
            {
                return 2;
            }
            return 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Nii1Header
        {
            public int sizeof_hdr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] data_type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public byte[] db_name;
            public int extents;
            public short session_error;
            public byte regular;
            public byte dim_info;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public short[] dim;
            public float intent_p1;
            public float intent_p2;
            public float intent_p3;
            public short intent_code;
            public short datatype;
            public short bitpix;
            public short slice_start;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public float[] pixdim;
            public float vox_offset;
            public float scl_slope;
            public float scl_inter;
            public short slice_end;
            public byte slice_code;
            public byte xyzt_units;
            public float cal_max;
            public float cal_min;
            public float slice_duration;
            public float toffset;
            public int glmax;
            public int glmin;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] descrip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] aux_file;
            public short qform_code;
            public short sform_code;
            public float quatern_b;
            public float quatern_c;
            public float quatern_d;
            public float qoffset_x;
            public float qoffset_y;
            public float qoffset_z;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] srow_x;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] srow_y;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] srow_z;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] intent_name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] magic;

            public Nii1Header()
            {
                this.data_type = new byte[10];
                this.db_name = new byte[18];
                this.dim = new short[8];
                this.pixdim = new float[8];
                this.descrip = new byte[80];
                this.aux_file = new byte[24];
                this.srow_x = new float[4];
                this.srow_y = new float[4];
                this.srow_z = new float[4];
                this.intent_name = new byte[16];
                this.magic = new byte[4];
            }
            
            public static implicit operator Nii1Header(Anal75Header analHeader)
            {
                Nii1Header niiHeader = new Nii1Header();
                IntPtr ptr = Marshal.AllocHGlobal(348);
                Marshal.StructureToPtr(analHeader, ptr, true);
                niiHeader = (Nii1Header)Marshal.PtrToStructure(ptr, niiHeader.GetType());
                Marshal.FreeHGlobal(ptr);
                return niiHeader;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Nii1Extender
        {
            public byte[] extension;
            public Nii1Extender()
            {
                extension = new byte[4];
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Nii1Extension
        {
            public int esize;
            public int ecode;
            public byte[] edata;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Anal75Header
        {
            public int sizeof_hdr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] data_type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public byte[] db_name;
            public int extents;
            public short session_error;
            public byte regular;
            public byte hkey_un0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public short[] dim;
            public short unused8;
            public short unused9;
            public short unused10;
            public short unused11;
            public short unused12;
            public short unused13;
            public short unused14;
            public short datatype;
            public short bitpix;
            public short dim_un0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public float[] pixdim;
            public float vox_offset;
            public float funused1;
            public float funused2;
            public float funused3;
            public float cal_max;
            public float cal_min;
            public float compressed;
            public float verified;
            public int glmax, glmin;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] descrip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] aux_file;
            public byte orient;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] originator;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] generated;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] scannum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] patient_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] exp_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] exp_time;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] hist_un0;
            public int views;
            public int vols_added;
            public int start_field;
            public int field_skip;
            public int omax, omin;
            public int smax, smin;

            public Anal75Header()
            {
                this.data_type = new byte[10];
                this.db_name = new byte[18];
                this.dim = new short[8];
                this.pixdim = new float[8];
                this.descrip = new byte[80];
                this.aux_file = new byte[24];
                this.originator = new byte[10];
                this.generated = new byte[10];
                this.scannum = new byte[10];
                this.patient_id = new byte[10];
                this.exp_date = new byte[10];
                this.exp_time = new byte[10];
                this.hist_un0 = new byte[3];
            }

            public static implicit operator Anal75Header(Nii1Header niiHeader)
            {
                Anal75Header analHeader = new Anal75Header();
                IntPtr ptr = Marshal.AllocHGlobal(348);
                Marshal.StructureToPtr(niiHeader, ptr, true);
                analHeader = (Anal75Header)Marshal.PtrToStructure(ptr, analHeader.GetType());
                Marshal.FreeHGlobal(ptr);
                return analHeader;
            }
        }

        public class NiiInfo
        {
            public int nx { get { return header.dim[1]; } }
            public int ny { get { return header.dim[2]; } }
            public int nz { get { return header.dim[3]; } }
            public int nt { get { return header.dim[4]; } }
            public int nu { get { return header.dim[5]; } }
            public int nv { get { return header.dim[6]; } }
            public int nw { get { return header.dim[7]; } }
            public int ndim { get { return header.dim[0]; } }
            public float dx { get { return header.pixdim[1]; } }
            public float dy { get { return header.pixdim[2]; } }
            public float dz { get { return header.pixdim[3]; } }
            public float dt { get { return header.pixdim[4]; } }
            public float du { get { return header.pixdim[5]; } }
            public float dv { get { return header.pixdim[6]; } }
            public float dw { get { return header.pixdim[7]; } }
            public float qfac { get { return header.pixdim[0]; } }
            public int numVoxels;
            public int bytesPerVoxel;
            public int bytesPerChannel;
            public int numChannels;
            public int freqDim;
            public int phaseDim;
            public int sliceDim;
            public int xyzUnits;
            public int timeUnits;
            public int byteOrder;
            public bool needsSwapping;
            public string hdrName;
            public Nii1Header header;
            public int niiVersion;
            public FormatCode format;
            public Nii1Extension[] extension;
            public int numExt;
            public string imgName;
            public int imgOffset;
            public anal75OrientCode analyze75Orient;

            public NiiInfo()
            {
                header = new Nii1Header();
            }

            public int getNiiInfo(string fName)
            {
                int fileCount = findFiles(fName);
                //Valid header?
                if (fileCount == 0 || readNii1Header(hdrName) == 0)
                {
                    return 0;
                }
                //Does header need a byte swap?
                needsSwapping = NIFTI_NEEDS_SWAP(header);
                //Is header NIfTI or analyze?
                int hdrType = getHeaderType(header);
                if (needsSwapping)
                {
                    switch (hdrType)
                    {
                        case 1:
                            swapNii1Header(ref header);
                            break;
                        case 2:
                            Anal75Header analHeader = header;
                            swapAnal75Header(ref analHeader);
                            header = analHeader;
                            break;
                        default:
                            return 0;
                    }
                }
                //Offset for NIfTI needs to be at least 352
                if (fileCount == 1 && header.vox_offset < 352.0f)
                {
                    header.vox_offset = 352.0f;
                }else if(fileCount == 2)
                {
                    header.vox_offset = 0;
                }
                //Setup image members
                if (imageSetup() == 0)
                {
                    return 0;
                }
                //Read NIfTI extensions
                BinaryReader extReader = new BinaryReader(File.Open(hdrName, FileMode.Open));
                if (extReader != null && (hdrName.Equals(imgName, StringComparison.Ordinal) || extReader.BaseStream.Length >= 352))
                {
                    extReader.BaseStream.Seek(348, SeekOrigin.Begin);
                    Nii1Extender extr = new Nii1Extender();
                    extr.extension = extReader.ReadBytes(4);
                    if (extr.extension[0] == 1)
                    {
                        int offset = 352;
                        List<Nii1Extension> extList = new List<Nii1Extension>();
                        while (offset < imgOffset)
                        {
                            Nii1Extension ext = new Nii1Extension();
                            ext.esize = extReader.ReadInt32();
                            if (ext.esize % 16 != 0)
                            {
                                Debug.Log("Extension.esize not multiple of 16!");
                            }
                            offset += ext.esize;
                            ext.ecode = extReader.ReadInt32();
                            ext.edata = extReader.ReadBytes(ext.esize - 8);
                            extList.Add(ext);
                        }
                        numExt = extList.Count;
                        extension = new Nii1Extension[numExt];
                        for (int i = 0; i < numExt; i++)
                        {
                            extension[i] = extList[i];
                        }
                    }
                    extReader.BaseStream.Close();
                    extReader.Close();
                }
                return 1;
            }

            int readNii1Header(string fName)
            {
                if (!File.Exists(fName))
                {
                    return 0;
                }
                try
                {
                    BinaryReader reader = new BinaryReader(File.Open(fName, FileMode.Open));
                    if (reader != null)
                    {
                        header.sizeof_hdr = reader.ReadInt32();
                        header.data_type = reader.ReadBytes(10);
                        header.db_name = reader.ReadBytes(18);
                        header.extents = reader.ReadInt32();
                        header.session_error = reader.ReadInt16();
                        header.regular = reader.ReadByte();
                        header.dim_info = reader.ReadByte();
                        Buffer.BlockCopy(reader.ReadBytes(2 * 8), 0, header.dim, 0, 2 * 8);
                        header.intent_p1 = reader.ReadSingle();
                        header.intent_p2 = reader.ReadSingle();
                        header.intent_p3 = reader.ReadSingle();
                        header.intent_code = reader.ReadInt16();
                        header.datatype = reader.ReadInt16();
                        header.bitpix = reader.ReadInt16();
                        header.slice_start = reader.ReadInt16();
                        Buffer.BlockCopy(reader.ReadBytes(4 * 8), 0, header.pixdim, 0, 4 * 8);
                        header.vox_offset = reader.ReadSingle();
                        header.scl_slope = reader.ReadSingle();
                        header.scl_inter = reader.ReadSingle();
                        header.slice_end = reader.ReadInt16();
                        header.slice_code = reader.ReadByte();
                        header.xyzt_units = reader.ReadByte();
                        header.cal_max = reader.ReadSingle();
                        header.cal_min = reader.ReadSingle();
                        header.slice_duration = reader.ReadSingle();
                        header.toffset = reader.ReadSingle();
                        header.glmax = reader.ReadInt32();
                        header.glmin = reader.ReadInt32();
                        header.descrip = reader.ReadBytes(80);
                        header.aux_file = reader.ReadBytes(24);
                        header.qform_code = reader.ReadInt16();
                        header.sform_code = reader.ReadInt16();
                        header.quatern_b = reader.ReadSingle();
                        header.quatern_c = reader.ReadSingle();
                        header.quatern_d = reader.ReadSingle();
                        header.qoffset_x = reader.ReadSingle();
                        header.qoffset_y = reader.ReadSingle();
                        header.qoffset_z = reader.ReadSingle();
                        Buffer.BlockCopy(reader.ReadBytes(4 * 4), 0, header.srow_x, 0, 4 * 4);
                        Buffer.BlockCopy(reader.ReadBytes(4 * 4), 0, header.srow_y, 0, 4 * 4);
                        Buffer.BlockCopy(reader.ReadBytes(4 * 4), 0, header.srow_z, 0, 4 * 4);
                        header.intent_name = reader.ReadBytes(16);
                        header.magic = reader.ReadBytes(4);
                        reader.BaseStream.Close();
                        reader.Close();
                        //348 bytes
                    }
                }
                catch (IOException)                 {   Debug.Log("IOException: " + fName);                     return 0; }
                catch (UnauthorizedAccessException) {   Debug.Log("UnauthorizedAccessException: " + fName);     return 0; }
                catch (NotSupportedException)       {   Debug.Log("NotSupportedException: " + fName);           return 0; }
                catch (ArgumentException)           {   Debug.Log("ArgumentException: " + fName);               return 0; }
                return 1;
            }

            int findFiles(string fName)
            {
                hdrName = null;
                imgName = null;
                string fExt = fName.Substring(fName.Length - 4, 4);
                string fBase = fName.Substring(0, fName.Length - 4);
                if (fExt.Equals(".nii", StringComparison.OrdinalIgnoreCase) && File.Exists(fBase + ".nii"))
                {
                    hdrName = fName;
                    imgName = fName;
                    return 1;
                }
                else if ((fExt.Equals(".hdr", StringComparison.OrdinalIgnoreCase) || fExt.Equals(".img", StringComparison.OrdinalIgnoreCase)) && File.Exists(fBase + ".hdr") && File.Exists(fBase + ".img"))
                {
                    hdrName = fBase + ".hdr";
                    imgName = fBase + ".img";
                    return 2;
                }
                return 0;
            }

            int imageSetup()
            {
                niiVersion = NIFTI_VERSION(header);

                if (niiVersion == 0)
                {
                    analyze75Orient = (anal75OrientCode)((Anal75Header)header).orient;
                }
                format = (FormatCode)header.datatype;
                if (format == FormatCode.NIFTI_TYPE_BINARY || format == FormatCode.NIFTI_TYPE_UNKNOWN)
                {
                    return 0;
                }
                if (header.dim[1] <= 0)
                {
                    return 0;
                }
                for (int i = 2; i <= header.dim[0]; i++)
                {
                    if (header.dim[i] <= 0)
                    {
                        header.dim[i] = 1;
                    }
                }
                for (int i = header.dim[0] + 1; i <= 7; i++)
                {
                    header.dim[i] = 0;
                }
                for (int i = 1; i <= header.dim[0]; i++)
                {
                    if (header.pixdim[i] == 0.0 || float.IsInfinity(header.pixdim[i]) || float.IsNaN(header.pixdim[i]))
                    {
                        header.pixdim[i] = 1.0f;
                    }
                }
                byteOrder = getByteOrder();
                numVoxels = 1;
                for (int i = 1; i <= header.dim[0]; i++)
                {
                    numVoxels *= header.dim[i];
                }
                getFormatSize(format, out bytesPerVoxel, out bytesPerChannel, out numChannels);
                if (bytesPerVoxel == 0)
                {
                    return 0;
                }
                if (niiVersion > 0)
                {
                    header.quatern_b = FIXED_FLOAT(header.quatern_b);
                    header.quatern_c = FIXED_FLOAT(header.quatern_c);
                    header.quatern_d = FIXED_FLOAT(header.quatern_d);
                    header.qoffset_x = FIXED_FLOAT(header.qoffset_x);
                    header.qoffset_y = FIXED_FLOAT(header.qoffset_y);
                    header.qoffset_z = FIXED_FLOAT(header.qoffset_z);
                    header.scl_slope = FIXED_FLOAT(header.scl_slope);
                    header.scl_inter = FIXED_FLOAT(header.scl_inter);
                    header.intent_p1 = FIXED_FLOAT(header.intent_p1);
                    header.intent_p2 = FIXED_FLOAT(header.intent_p2);
                    header.intent_p3 = FIXED_FLOAT(header.intent_p3);
                    header.toffset = FIXED_FLOAT(header.toffset);
                    xyzUnits = XYZT_TO_SPACE(header.xyzt_units);
                    timeUnits = XYZT_TO_TIME(header.xyzt_units);
                    freqDim = DIM_INFO_TO_FREQ_DIM(header.dim_info);
                    phaseDim = DIM_INFO_TO_PHASE_DIM(header.dim_info);
                    sliceDim = DIM_INFO_TO_SLICE_DIM(header.dim_info);
                    header.slice_duration = FIXED_FLOAT(header.slice_duration);
                }
                header.cal_min = FIXED_FLOAT(header.cal_min);
                header.cal_max = FIXED_FLOAT(header.cal_max);
                imgOffset = (int)header.vox_offset;
                numExt = 0;
                extension = null;
                return 1;
            }
        }
    }

    public class NII2Volume
    {
        public Volume volume;
        public NIfTI.NiiInfo niiInfo;

        public IEnumerator loadFile(string fName, VolumeTextureFormat forceTextureFormat, FloatEvent loadingProgressChanged, PlanarConfiguration planarConfig, Ref<bool> completed, Ref<int> returned)
        {
            returned.val = 1;
            fName = Path.GetFullPath(fName);
            if (!File.Exists(fName))
            {
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            niiInfo = new NIfTI.NiiInfo();
            if (niiInfo.getNiiInfo(fName) == 0)
            {
                Debug.Log("Unable to obtain NIfTI info for " + fName);
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            int numPixels = niiInfo.nx * niiInfo.ny;
            volume = new Volume();
            //volume.nx = Mathf.NextPowerOfTwo(niiInfo.nx);
            //volume.ny = Mathf.NextPowerOfTwo(niiInfo.ny);
            //volume.nz = Mathf.NextPowerOfTwo(niiInfo.nz);
            volume.nx = niiInfo.nx;
            volume.ny = niiInfo.ny;
            volume.nz = niiInfo.nz;
            volume.dx = niiInfo.dx;
            volume.dy = niiInfo.dy;
            volume.dz = niiInfo.dz;
            int startX = (int)Mathf.Floor((volume.nx - niiInfo.nx) / 2.0f);
            int startY = (int)Mathf.Floor((volume.ny - niiInfo.ny) / 2.0f);
            int startZ = (int)Mathf.Floor((volume.nz - niiInfo.nz) / 2.0f);
            volume.numVoxels = volume.nx * volume.ny * volume.nz;
            Color[] volColors = null;
            Color32[] volColors32 = null;
            float progressMultiplier = 1;
            switch (niiInfo.format)
            {
                case NIfTI.FormatCode.NIFTI_TYPE_INT8:
                case NIfTI.FormatCode.NIFTI_TYPE_UINT8:
                    volume.format = TextureFormat.Alpha8;
                    volColors32 = new Color32[volume.numVoxels];
                    break;
                case NIfTI.FormatCode.NIFTI_TYPE_INT16:
                case NIfTI.FormatCode.NIFTI_TYPE_UINT16:
                    volume.format = TextureFormat.RHalf;
                    volColors = new Color[volume.numVoxels];
                    progressMultiplier = 0.5f;
                    break;
                case NIfTI.FormatCode.NIFTI_TYPE_INT32:
                case NIfTI.FormatCode.NIFTI_TYPE_UINT32:
                case NIfTI.FormatCode.NIFTI_TYPE_FLOAT32:
                    volume.format = TextureFormat.RFloat;
                    volColors = new Color[volume.numVoxels];
                    progressMultiplier = 0.5f;
                    break;
                case NIfTI.FormatCode.NIFTI_TYPE_RGB24:
                    volume.format = TextureFormat.RGB24;
                    volColors32 = new Color32[volume.numVoxels];
                    break;
                case NIfTI.FormatCode.NIFTI_TYPE_RGBA32:
                    volume.format = TextureFormat.RGBA32;
                    volColors32 = new Color32[volume.numVoxels];
                    break;
                default:
                    Debug.Log("Format not supported: " + niiInfo.format);
                    returned.val = 0;
                    completed.val = true;
                    yield break;
            }
            switch (forceTextureFormat)
            {
                case VolumeTextureFormat.Alpha8:
                    volume.format = TextureFormat.Alpha8;
                    break;
                case VolumeTextureFormat.RFloat:
                    volume.format = TextureFormat.RFloat;
                    break;
                case VolumeTextureFormat.RHalf:
                    volume.format = TextureFormat.RHalf;
                    break;
                case VolumeTextureFormat.RGB24:
                    volume.format = TextureFormat.RGB24;
                    break;
                case VolumeTextureFormat.RGBA32:
                    volume.format = TextureFormat.RGBA32;
                    break;
                case VolumeTextureFormat.ARGB32:
                    volume.format = TextureFormat.ARGB32;
                    break;
                case VolumeTextureFormat.RGBAFloat:
                    volume.format = TextureFormat.RGBAFloat;
                    break;
                case VolumeTextureFormat.RGBAHalf:
                    volume.format = TextureFormat.RGBAHalf;
                    break;
            }
            try
            {
                volume.texture = new Texture3D(volume.nx, volume.ny, volume.nz, volume.format, false);
            }
            catch (UnityException)
            {
                Debug.Log("Couldn't create Texture3D(" + volume.nx + ", " + volume.ny + ", " + volume.nz + ", " + volume.format + ", " + "false).\nTry forcing a different format in the VolumeFileLoader component.");
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            if (volume.texture == null)
            {
                Debug.Log("Couldn't create Texture3D(" + volume.nx + ", " + volume.ny + ", " + volume.nz + ", " + volume.format + ", " + "false).\nTry forcing a different format in the VolumeFileLoader component.");
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            BinaryReader imgReader = new BinaryReader(File.Open(niiInfo.imgName, FileMode.Open));
            imgReader.BaseStream.Seek(niiInfo.imgOffset, SeekOrigin.Begin);
            int numBytesPerSlice = niiInfo.bytesPerVoxel * numPixels;
            byte[] slice = new byte[numBytesPerSlice];
            int stopZ = startZ + niiInfo.nz;
            int stopY = startY + niiInfo.ny;
            int stopX = startX + niiInfo.nx;
            float maxValue = 0;
            float minValue = Mathf.Infinity;
            int cumZ, cumY, index;
            for (int z = startZ; z < stopZ; z++)
            {
                cumZ = z * volume.ny * volume.nx;
                slice = imgReader.ReadBytes(numBytesPerSlice);
                if (niiInfo.needsSwapping && niiInfo.bytesPerChannel > 1)
                {
                    slice = NIfTI.swapBytes(numBytesPerSlice / niiInfo.bytesPerChannel, niiInfo.bytesPerChannel, slice);
                }
                int byteIndex = 0;
                switch (niiInfo.format)
                {
                    case NIfTI.FormatCode.NIFTI_TYPE_INT8:
                        byte valueByte;
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                valueByte = slice[byteIndex] > 127 ? (byte)(slice[byteIndex] - 128) : (byte)(slice[byteIndex] + 128);
                                volColors32[cumZ + cumY + x] = new Color32(valueByte, 0, 0, valueByte);
                                byteIndex++;
                            }
                        }
                        break;
                    case NIfTI.FormatCode.NIFTI_TYPE_UINT8:
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                volColors32[cumZ + cumY + x] = new Color32(slice[byteIndex], 0, 0, slice[byteIndex]);
                                byteIndex++;
                            }
                        }
                        break;
                    case NIfTI.FormatCode.NIFTI_TYPE_INT16:
                        Int16 valueInt16;
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                valueInt16 = BitConverter.ToInt16(slice, byteIndex);
                                index = cumZ + cumY + x;
                                //volColors[index] = new Color(valueUInt16 > 32767 ? (valueUInt16 - 32768) : (valueUInt16 + 32768), 0, 0, 0);
                                volColors[index] = new Color(valueInt16, 0, 0, 0);
                                if (maxValue < volColors[index].r)
                                {
                                    maxValue = volColors[index].r;
                                }
                                if (minValue > volColors[index].r)
                                {
                                    minValue = volColors[index].r;
                                }
                                byteIndex += 2;
                            }
                        }
                        break;
                    case NIfTI.FormatCode.NIFTI_TYPE_UINT16:
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                index = cumZ + cumY + x;
                                volColors[index] = new Color(BitConverter.ToUInt16(slice, byteIndex), 0, 0, 0);
                                if (maxValue < volColors[index].r)
                                {
                                    maxValue = volColors[index].r;
                                }
                                if (minValue > volColors[index].r)
                                {
                                    minValue = volColors[index].r;
                                }
                                byteIndex += 2;
                            }
                        }
                        break;
                    case NIfTI.FormatCode.NIFTI_TYPE_INT32:
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                index = cumZ + cumY + x;
                                volColors[index] = new Color(BitConverter.ToInt32(slice, byteIndex), 0, 0, 0);
                                if (maxValue < volColors[index].r)
                                {
                                    maxValue = volColors[index].r;
                                }
                                if (minValue > volColors[index].r)
                                {
                                    minValue = volColors[index].r;
                                }
                                byteIndex += 4;
                            }
                        }
                        break;
                    case NIfTI.FormatCode.NIFTI_TYPE_UINT32:
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                index = cumZ + cumY + x;
                                volColors[index] = new Color(BitConverter.ToUInt32(slice, byteIndex), 0, 0, 0);
                                if (maxValue < volColors[index].r)
                                {
                                    maxValue = volColors[index].r;
                                }
                                if (minValue > volColors[index].r)
                                {
                                    minValue = volColors[index].r;
                                }
                                byteIndex += 4;
                            }
                        }
                        break;
                    case NIfTI.FormatCode.NIFTI_TYPE_FLOAT32:
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                index = cumZ + cumY + x;
                                volColors[index] = new Color(BitConverter.ToSingle(slice, byteIndex), 0, 0, 0);
                                if (maxValue < volColors[index].r)
                                {
                                    maxValue = volColors[index].r;
                                }
                                if (minValue > volColors[index].r)
                                {
                                    minValue = volColors[index].r;
                                }
                                byteIndex += 4;
                            }
                        }
                        break;
                    case NIfTI.FormatCode.NIFTI_TYPE_RGB24:
                        if (planarConfig == PlanarConfiguration.Separated)
                        {
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x] = new Color32(slice[byteIndex++], 0, 0, 255);
                                }
                            }
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x].g = slice[byteIndex++];
                                }
                            }
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x].b = slice[byteIndex++];
                                }
                            }
                        }
                        else
                        {
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x] = new Color32(slice[byteIndex++], slice[byteIndex++], slice[byteIndex++], 255);
                                }
                            }
                        }
                        break;
                    case NIfTI.FormatCode.NIFTI_TYPE_RGBA32:
                        if (planarConfig == PlanarConfiguration.Separated)
                        {
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x] = new Color32(slice[byteIndex++], 0, 0, 0);
                                }
                            }
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x].g = slice[byteIndex++];
                                }
                            }
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x].b = slice[byteIndex++];
                                }
                            }
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x].a = slice[byteIndex++];
                                }
                            }
                        }
                        else
                        {
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x] = new Color32(slice[byteIndex++], slice[byteIndex++], slice[byteIndex++], slice[byteIndex++]);
                                }
                            }
                        }
                        break;
                }
                loadingProgressChanged.Invoke(progressMultiplier * (z - startZ + 1) / niiInfo.nz);
                yield return null;
            }
            imgReader.BaseStream.Close();
            imgReader.Close();
            switch (niiInfo.format)
            {
                case NIfTI.FormatCode.NIFTI_TYPE_INT8:
                case NIfTI.FormatCode.NIFTI_TYPE_UINT8:
                case NIfTI.FormatCode.NIFTI_TYPE_RGB24:
                case NIfTI.FormatCode.NIFTI_TYPE_RGBA32:
                    volume.texture.SetPixels32(volColors32);
                    break;
                case NIfTI.FormatCode.NIFTI_TYPE_INT16:
                case NIfTI.FormatCode.NIFTI_TYPE_UINT16:
                case NIfTI.FormatCode.NIFTI_TYPE_INT32:
                case NIfTI.FormatCode.NIFTI_TYPE_UINT32:
                case NIfTI.FormatCode.NIFTI_TYPE_FLOAT32:
                    //volume.texture.SetPixels(volColors);

                    float valueRange = maxValue - minValue;
                    for (int z = startZ; z < stopZ; z++)
                    {   
                        cumZ = z * volume.ny * volume.nx;
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                index = cumZ + cumY + x;
                                //volColors[index].r = (volColors[index].r - minValue) / valueRange;
                                //volColors[index].g = volColors[index].r;
                                //volColors[index].b = volColors[index].r;
                                volColors[index].a = 1;
                            }
                        }
                        loadingProgressChanged.Invoke(0.5f + progressMultiplier * (z - startZ + 1) / niiInfo.nz);
                        yield return null;
                    }
                    volume.texture.SetPixels(volColors);
                    break;
            }
            volume.texture.filterMode = FilterMode.Bilinear;
            volume.texture.wrapMode = TextureWrapMode.Clamp;
            volume.texture.Apply();
            volume.niiInfo=niiInfo;
            completed.val = true;
        }
    }
}
#endif