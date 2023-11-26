using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class TrackerMath
{
    private int indiceAtual = 0;
    private int maximoItensPorLinha = 12; //PODE SER ALTERADO
    private int exibirApenasImpares = 0; //PODE SER ALTERADO

    private float[] vive_ub;
    private float[] vive_uc;
    
    // Posição INICIAL ORIGEM Caixa B
    private Vector3 PO;

    // Posição INICIAL ORIGEM Caixa C
    private Vector3 QO;

    //(diferença entre as origens da linha)
    private Vector3 WO;

    public bool IsFlipped { get; set; }

    public TrackerMath(Vector3 PO, Vector3 QO, float[] vive_ub, float[] vive_uc)
    {
        IsFlipped = PO.x < 0;

        this.PO = PO;
        this.QO = QO;
        
        this.vive_ub = vive_ub;
        this.vive_uc = vive_uc;
        
        WO = PO - QO;
    }

    // Aqui ira conter os calculos apos o final de um ciclo
    public Vector3 RealizarCalculos(float angle_hor_b, float angle_ver_b, float angle_hor_c, float angle_ver_c)
    {
        //*****PASSO 2*****
        //MOSTRA NO SERIAL MONITOR RESULTADO float V_hor[3] XB
        Vector3 V_horB = new Vector3(Mathf.Cos(angle_hor_b),0,Mathf.Sin(angle_hor_b));

        //MOSTRA NO SERIAL MONITOR RESULTADO float V_ver[3] YB
        Vector3 V_verB = new Vector3(0,Mathf.Cos(angle_ver_b),-Mathf.Sin(angle_ver_b));
        
        //MOSTRA NO SERIAL MONITOR RESULTADO float V_hor[3] XB
        Vector3 V_horC = new Vector3(Mathf.Cos(angle_hor_c),0,Mathf.Sin(angle_hor_c));
        
        //MOSTRA NO SERIAL MONITOR RESULTADO float V_ver[3] YB
        Vector3 V_verC = new Vector3(0,Mathf.Cos(angle_ver_c),-Mathf.Sin(angle_ver_c));

        //*****PASSO 3*****
        //PRODUTO VETORIAL CAIXA B
        Vector3 vetorR_UB = Vector3.Cross(V_horB, V_verB);

        //PRODUTO VETORIAL CAIXA C
        Vector3 vetorR_UC = Vector3.Cross(V_horC, V_verC);

        //NORMALIZACAO DE VETOR CAIXA B
        Vector3 normalVetor_UB = vetorR_UB.normalized;

        //NORMALIZACAO DE VETOR CAIXA C
        Vector3 normalVetor_UC = vetorR_UC.normalized;

        //*****PASSO 4*****                    
        //MULTIPLICACAO DE MATRIZ 3x3 3x1 CAIXA B 
        Vector3 matriz_UB = MultMatriz(vive_ub, normalVetor_UB);
        
        //MULTIPLICACAO DE MATRIZ 3x3 3x1 CAIXA C
        Vector3 matriz_UC = MultMatriz(vive_uc, normalVetor_UC);

        //*****PASSO 5*****
        //Equações de linha: P(s) = P0 + s*U and Q(t) = Q0 + t*V
        //Onde s, t são parâmetros escalares, pontos de origem da linha P0, Q0 
        //(nos nossos centros de estação de caso), vetores de linha U, V.

        //Produto Escalar
        float a = Vector3.Dot(matriz_UB, matriz_UB);

        float b = Vector3.Dot(matriz_UB, matriz_UC);

        float c = Vector3.Dot(matriz_UC, matriz_UC);

        float d = Vector3.Dot(matriz_UB, WO);

        float e = Vector3.Dot(matriz_UC, WO);
        
        float denom = a * c - b * b;
        float s = (b * e - c * d) / denom;
        float t = (a * e - b * d) / denom;

        //-------------------------------
        //Final_Point[3]= W0 + (s * u) - (t * v);
        //-------------------------------

        //-------------------------------             
        //P(s) = P0 + s*U OU Q(t) = Q0 + t*V
        //-------------------------------

        Vector3 P_s = new Vector3(
            matriz_UB[0] * s + PO[0],
            matriz_UB[1] * s + PO[1],
            matriz_UB[2] * s + PO[2]);
        
        Vector3 Q_s = new Vector3(
            matriz_UC[0] * t + QO[0],
            matriz_UC[1] * t + QO[1],
            matriz_UC[2] * t + QO[2]);

        //--------------------------------
        //Final_Point = ((P(s) + Q(t)) / 2);
        //--------------------------------
        
        return new Vector3(
            (P_s[0] + Q_s[0]) / 2,
            (P_s[1] + Q_s[1]) / 2,
            (P_s[2] + Q_s[2]) / 2);
    }
    
    //MATRIZ 3X3 * 3X1
    private Vector3 MultMatriz(float[] a, Vector3 b)
    {
        Vector3 resultado = Vector3.zero;
        
        resultado[0] = a[0] * b[0] + a[1] * b[1] + a[2] * b[2]; //(X)
        resultado[1] = a[3] * b[0] + a[4] * b[1] + a[5] * b[2]; //(Y)
        resultado[2] = a[6] * b[0] + a[7] * b[1] + a[8] * b[2]; //(Z)

        return resultado;
    }
}