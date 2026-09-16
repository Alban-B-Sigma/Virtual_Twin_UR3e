using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script was a first version of the "IKCalculator" script but its algorythm cannot give correct joint angles, it contains also some //
// calculation functions.                                                                                                                 //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class Calculs : MonoBehaviour //This script contains all functions that are used for calculations
{                                    // of the geometric model of the robot or operations on matrix
    // Here the distance between each articulations (Denavit-Hartenberg parameters)
    public double d1 = 0.1519; public double a2 = -0.24355; public double a3 = -0.2132; //public double d1 = 0.1519; public double a2 = -0.24355; public double a3 = -0.2132;
    public double d4 = 0.13105; public double d5 = 0.08535; public double d6 = 0.0921; //public double d4 = 0.13105; public double d5 = -0.08535; public double d6 = 0.0921;
    private int prec = 4;
    public List<double> ModuloPi(List<double> Q) // This function return in which half tour each articulation angles are, tout adjust it
    {                                            // due to the periodic nature of trigonometric functions
        List<double> L = new List<double>{Math.Round(Q[0]/math.TAU), 0, 0, 0, Math.Floor(Q[4]/math.PI), Math.Floor(Q[5]/math.PI)};
        return L;
    }
    public List<double> MGI_Mat(double4x4 matH, List<double> modulo)    // Set up the Inverse Geoletric Model (MGI in French) function,
    {                                                                   // when it is called with a matrix as entry
        // Extraction of the position coordinates
        double px = matH.c3.x;
        double py = matH.c3.y;
        double pz = matH.c3.z;

        // Extraction of the rotation matrix
        double4 col0 = matH.c0; double4 col1 = matH.c1; double4 col2 = matH.c2;
        double3x3 R = new double3x3(col0.x, col1.x, col2.x,
                                    col0.y, col1.y, col2.y,
                                    col0.z, col1.z, col2.z);
        
        return ResolutionMGI(px, py, pz, R, modulo);
    }
    public List<double> MGI_ang_pos(double px, double py, double pz, double psi, double theta, double phi, List<double> modulo)
    
    { // Set up the MGI when it is called with, as entries, the positions coordinates and the euler angles
        // Rotation matrices for each Euler angle
        double3x3 R1 = new double3x3(   Math.Cos(psi), -Math.Sin(psi), 0,
                                        Math.Sin(psi), Math.Cos(psi),  0,
                                        0,             0,              1    );
        double3x3 R2 = new double3x3(   1, 0,               0,
                                        0, Math.Cos(theta), -Math.Sin(theta),
                                        0, Math.Sin(theta),  Math.Cos(theta));
        double3x3 R3 = new double3x3(   Math.Cos(phi), -Math.Sin(phi), 0,
                                        Math.Sin(phi),  Math.Cos(phi), 0,
                                        0,             0,             1     );
        // Multiplication of the three matrices R = R1 * R2 * R3
        double3x3 R = MulMat3x3(R1, MulMat3x3(R2, R3));

        return ResolutionMGI(px, py, pz, R, modulo);
    }
    private List<double> ResolutionMGI(double px, double py, double pz, double3x3 R_dirt, List<double> modulo)
    {   // Calculation of the joint angles from the TCP position and orientation
        // Adjust the rotation Matrix to make it orthogonal, this is optional and try to make the calculation more precise
        // double3x3 R = Norm3(R_dirt);
        double3x3 R = R_dirt; //If you comment these orthogonalisation steps, use this line
        // Extraction of rotation matrix coefficients
        double r11 = Rd(R.c0.x); double r12 = Rd(R.c1.x); double r13 = Rd(R.c2.x);
        double r21 = Rd(R.c0.y); double r22 = Rd(R.c1.y); double r23 = Rd(R.c2.y);
        double r31 = Rd(R.c0.z); double r32 = Rd(R.c1.z); double r33 = Rd(R.c2.z);
        px = Rd(px); py = Rd(py); pz = Rd(pz); 

        // --- Calculation of the q1 angle --- \\
        double A = Rd(px - d6 * r13);
        double B = Rd(-(py - d6 * r23));
        double C = Rd(-d4);
        double delta1 = A * A + B * B - C * C;
        // double t1 = (A + Math.Sqrt(delta1)) / (B - C);
        // You can also use the Atan2(x, y) function instead of Atan(x/y)
        double q1 = Rd(2 * Math.Atan(Rd((A - Math.Sqrt(Rd(Math.Abs(delta1))))/(B - C))));
        // Adjust the angle with the number of half tour
        if (modulo[0] % 2 != 0) {q1 += Rd(Math.Sign(modulo[0])*math.TAU);}
        // Make sure the angle stay in the right interval
        q1 = Math.Clamp(q1, -math.TAU, math.TAU);
        q1 = Rd(q1);

        // --- Calculation of the q5 angle --- \\
        double cosQ5 = -r13 * Math.Sin(q1) + r23 * Math.Cos(q1);
        // Security of the Acos domain [-1, 1]
        // cosQ5 = Math.Clamp(cosQ5, -1.0, 1.0);
        double q5 = Math.Acos(cosQ5);
        if (modulo[4]%2 ==0) {q5 = -q5 + math.sign(modulo[4]+1)*math.PI;}
        else {q5 += math.sign(modulo[4])*math.PI;}
        q5 = Math.Clamp(q5, -math.TAU, math.TAU);

        // --- Calculation of the q6 angle --- \\
        double s6 = -r11 * r33 * Math.Cos(q1) - r21 * r33 * Math.Sin(q1) + r31 * (r23 * Math.Sin(q1) + r13 * Math.Cos(q1));
        double c6 = -r12 * r33 * Math.Cos(q1) - r22 * r33 * Math.Sin(q1) + r32 * (r23 * Math.Sin(q1) + r13 * Math.Cos(q1));
        double q6 = Math.Atan2(s6, c6);
        if (modulo[4]%2 == 0)
        {
            //if (modulo[5]<0)
            //{q6 -= math.PI;}
            //else
            //{q6 += math.PI;}
        }
        else
        {
            //if (modulo[5] == -2)
            //{q6 -= math.TAU;}
            //else if (modulo[5] == 2)
            //{q6 = math.TAU;}
            //else if (modulo[5] == 1)
            //{q6 += math.TAU;}
        }

        // --- Calculation of the sum q2+q3+q4 --- \\
        double s234 = Math.Sin(q6) * (r11 * Math.Cos(q1) + r21 * Math.Sin(q1)) + Math.Cos(q6) * (r12 * Math.Cos(q1) + r22 * Math.Sin(q1));
        double c234 = Math.Cos(q5) * (Math.Cos(q1) * (r11 * Math.Cos(q6) - r12 * Math.Sin(q6)) + Math.Sin(q1) * (r21 * Math.Cos(q6) - r22 * Math.Sin(q6))) - Math.Sin(q5) * (Math.Cos(q1) * r13 + Math.Sin(q1) * r23);
        double q234 = Math.Atan2(s234, c234);

        // --- Calculation of q2 angle and the sum q2+q3 --- \\
        double C1 = (d5 * Math.Cos(q6) * r12 + d5 * Math.Sin(q6) * r11 - d6 * r13 + px) * Math.Cos(q1) + Math.Sin(q1) * (d5 * Math.Cos(q6) * r22 + d5 * Math.Sin(q6) * r21 - d6 * r23 + py);
        double C2 = Math.Cos(q6) * d5 * r32 + Math.Sin(q6) * d5 * r31 - d6 * r33 - d1 + pz;
        
        double A23 = 2 * a2 * C2;
        double B23 = 2 * a2 * C1;
        double C23 = -(C1 * C1 + C2 * C2 + a2 * a2 - a3 * a3);
        double delta2 = A23 * A23 + B23 * B23 - C23 * C23;
        // If delta2 is <0, it may create errors
        if (delta2 < 0) {
            //Debug.LogWarning("Calculation of q2 is impossible");
            delta2 = 0; 
        }

        double q2 = 2 * Math.Atan2(A23 + Math.Sqrt(delta2) , B23 - C23); // or double q2 = 2 * Math.Atan2(A23 - Math.Sqrt(delta2) , B23 - C23);

        double s23 = (C2 - a2 * Math.Sin(q2)) / a3;
        double c23 = (C1 - a2 * Math.Cos(q2)) / a3;
        double q23 = Math.Atan2(s23, c23);

        // --- Calculation of the q3 and q4 angles --- \\
        double q3 = q23 - q2;
        double q4 = q234 - q23;

        return new List<double>{ q1, q2, q3, q4, q5, q6 };
    }
    public double4x4 MGD(List<double> q)
    { // Return the tool position and orientation matrix from the joint angles
        double q1 = q[0]; double q2 = q[1]; double q3 = q[2]; double q4 = q[3]; double q5 = q[4]; double q6 = q[5];
        double4x4 A01 = DH(q1, d1, 0, math.PI/2);   // Create all the transformation matrices to express
        double4x4 A12 = DH(q2, 0, a2, 0);           // the coordinate system of an articulation in the
        double4x4 A23 = DH(q3, 0, a3, 0);           // coordinate system of the previous articulation
        double4x4 A34 = DH(q4, d4, 0, math.PI/2);   // with the Denavit-Hartenberg (DH) parameters.
        double4x4 A45 = DH(q5, d5, 0, -math.PI/2);
        double4x4 A56 = DH(q6, d6, 0, 0);

        double4x4 Amgd = MulMat4x4(A01,MulMat4x4(A12,MulMat4x4(A23,MulMat4x4(A34,MulMat4x4(A45,A56)))));
        Amgd = Norm4(Amgd);
        return Amgd;
    }

    public double4x4 DH(double theta, double d, double a, double alf)
    { // Calculate the transformation matrix with the Denavit-Hartenberg parameters
        // Rotation matrix on the Z axe
        double4x4 Rz = new double4x4(Math.Cos(theta),-Math.Sin(theta), 0, 0,
                                     Math.Sin(theta), Math.Cos(theta), 0, 0,
                                     0, 0, 1, 0,
                                     0, 0, 0, 1);
        // Translation matrix on the Z axe
        double4x4 Tz = new double4x4(1, 0, 0, 0,
                                     0, 1, 0, 0,
                                     0, 0, 1, d,
                                     0, 0, 0, 1);
        // Translation matrix on the X axe
        double4x4 Tx = new double4x4(1, 0, 0, a,
                                     0, 1, 0, 0,
                                     0, 0, 1, 0,
                                     0, 0, 0, 1);
        // Rotation matrix on the X axe
        double4x4 Rx = new double4x4(1, 0, 0, 0,
                                     0, Math.Cos(alf),-Math.Sin(alf), 0,
                                     0, Math.Sin(alf), Math.Cos(alf), 0,
                                     0, 0, 0, 1);
        double4x4 Adh = MulMat4x4(Rz,MulMat4x4(Tz,MulMat4x4(Tx,Rx)));
        return Adh;
    }

    public double Rd(double dbl)
    {
        return Math.Round(dbl, prec);
    }


    //--------------------------------------------------Operation on Matrices----------------------------------------------------------\\
    private double3x3 MulMat3x3(double3x3 A, double3x3 B)
    { // Calculate the multiplication of two 3x3 matrices with the expression C[i][j] = A[i][x]*B[x][j]; x goes from 0 to 2
        double3x3 C = new double3x3(
            // Line 1 :
            A.c0.x*B.c0.x + A.c1.x*B.c0.y + A.c2.x*B.c0.z, 
            A.c0.x*B.c1.x + A.c1.x*B.c1.y + A.c2.x*B.c1.z, 
            A.c0.x*B.c2.x + A.c1.x*B.c2.y + A.c2.x*B.c2.z,
            // Line 2 :
            A.c0.y*B.c0.x + A.c1.y*B.c0.y + A.c2.y*B.c0.z, 
            A.c0.y*B.c1.x + A.c1.y*B.c1.y + A.c2.y*B.c1.z, 
            A.c0.y*B.c2.x + A.c1.y*B.c2.y + A.c2.y*B.c2.z,
            // Line 3 :
            A.c0.z*B.c0.x + A.c1.z*B.c0.y + A.c2.z*B.c0.z, 
            A.c0.z*B.c1.x + A.c1.z*B.c1.y + A.c2.z*B.c1.z, 
            A.c0.z*B.c2.x + A.c1.z*B.c2.y + A.c2.z*B.c2.z  );
        return C;
    }
    private double4x4 MulMat4x4(double4x4 A, double4x4 B)
    {
        double4x4 C = new double4x4(
            //Line 1 :
            A.c0.x*B.c0.x + A.c1.x*B.c0.y + A.c2.x*B.c0.z + A.c3.x*B.c0.w, 
            A.c0.x*B.c1.x + A.c1.x*B.c1.y + A.c2.x*B.c1.z + A.c3.x*B.c1.w, 
            A.c0.x*B.c2.x + A.c1.x*B.c2.y + A.c2.x*B.c2.z + A.c3.x*B.c2.w, 
            A.c0.x*B.c3.x + A.c1.x*B.c3.y + A.c2.x*B.c3.z + A.c3.x*B.c3.w,
            //Line 2 :
            A.c0.y*B.c0.x + A.c1.y*B.c0.y + A.c2.y*B.c0.z + A.c3.y*B.c0.w, 
            A.c0.y*B.c1.x + A.c1.y*B.c1.y + A.c2.y*B.c1.z + A.c3.y*B.c1.w, 
            A.c0.y*B.c2.x + A.c1.y*B.c2.y + A.c2.y*B.c2.z + A.c3.y*B.c2.w,
            A.c0.y*B.c3.x + A.c1.y*B.c3.y + A.c2.y*B.c3.z + A.c3.y*B.c3.w,
            //Line 3 :
            A.c0.z*B.c0.x + A.c1.z*B.c0.y + A.c2.z*B.c0.z + A.c3.z*B.c0.w, 
            A.c0.z*B.c1.x + A.c1.z*B.c1.y + A.c2.z*B.c1.z + A.c3.z*B.c1.w, 
            A.c0.z*B.c2.x + A.c1.z*B.c2.y + A.c2.z*B.c2.z + A.c3.z*B.c2.w,
            A.c0.z*B.c3.x + A.c1.z*B.c3.y + A.c2.z*B.c3.z + A.c3.z*B.c3.w,
            //Line 4 :
            A.c0.w*B.c0.x + A.c1.w*B.c0.y + A.c2.w*B.c0.z + A.c3.w*B.c0.w, 
            A.c0.w*B.c1.x + A.c1.w*B.c1.y + A.c2.w*B.c1.z + A.c3.w*B.c1.w, 
            A.c0.w*B.c2.x + A.c1.w*B.c2.y + A.c2.w*B.c2.z + A.c3.w*B.c2.w, 
            A.c0.w*B.c3.x + A.c1.w*B.c3.y + A.c2.w*B.c3.z + A.c3.w*B.c3.w
            );
        return C;
    }
    public double3 MatToVect(double4x4 Mat)
    { // Calculate the Euler angles from the rotation matrix
        // Extract the usefull coefficients from the matrix
                                                        double r13= Mat.c2.x;
                                                        double r23= Mat.c2.y;
        double r31= Mat.c0.z;   double r32= Mat.c1.z;   double r33= Mat.c2.z;

        double psi = Math.Atan2(r13,-r23);
        double theta = Math.Acos(r33);
        double phi = Math.Atan2(r31,r32);

        return new double3(psi, theta, phi);
    }
    public double4x4 Norm4(double4x4 MnNorm)
    {
        double3x3 R_dirt = new double3x3(MnNorm.c0.x, MnNorm.c1.x, MnNorm.c2.x,
                                    MnNorm.c0.y, MnNorm.c1.y, MnNorm.c2.y,
                                    MnNorm.c0.z, MnNorm.c1.z, MnNorm.c2.z);
        double3 vZ = math.normalize(R_dirt.c2); // Normalise the first axe
        double3 vX = math.normalize(R_dirt.c0 - math.dot(vZ, R_dirt.c0) * vZ); // Set the second axe as orthogonal to the first, then normalise it
        double3 vY = math.cross(vZ, vX); // The third axe is the vectorial product of the two first axes
        // Re-define the rotation matrix 
        double3x3 R = new double3x3(vX, vY, vZ);
        return new double4x4(R.c0.x,        R.c1.x,         R.c2.x,        MnNorm.c3.x,
                             R.c0.y,        R.c1.y,         R.c2.y,        MnNorm.c3.y,
                             R.c0.z,        R.c1.z,         R.c2.z,        MnNorm.c3.z,
                             MnNorm.c0.w,   MnNorm.c1.w,    MnNorm.c2.w,   MnNorm.c3.w);
    }
    public double3x3 Norm3(double3x3 R_dirt)
    {
        double3 vZ = math.normalize(R_dirt.c2); // Normalise the first axe
        double3 vX = math.normalize(R_dirt.c0 - math.dot(vZ, R_dirt.c0) * vZ); // Set the second axe as orthogonal to the first, then normalise it
        double3 vY = math.cross(vZ, vX); // The third axe is the vectorial product of the two first axes
        // Re-define the rotation matrix 
        double3x3 R = new double3x3(vX, vY, vZ);
        return R;
    }
}