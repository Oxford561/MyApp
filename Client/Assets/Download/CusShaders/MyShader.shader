Shader "Custom/ShowTexture" {
	Properties {
		_Color ("Color", Color) = (0,0,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
        Pass{
             Material {
                 Diffuse[_Color]             
             }
             Lighting On
             SetTexture[_MainTex]
             {
                Combine texture*primary, texture*constant   //combine color部分，alpha部分 ,材质 * 顶点颜色
             }
        }
	} 
	FallBack "Diffuse"
}
