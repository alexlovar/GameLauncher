   M S B A M L   `   `   `  ����   RFStasis_Launcher    rlgn.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  Launcher��$	��561��$
��1037��$	��562��$
��1037��$	��562�� � Icon$ Images/trinity.ico�� � ShowInTaskbar .  � Topmost .   � WindowStartupLocation$ CenterScreen=� � WindowStyle$
 None=� � AllowsTransparency . ���	#00000000$	��354�� � 
ResizeMode$ NoResize=�.��� $��0,14,-63,28q�.+��� -   $��imBackground��$	��523��$
��Left=�$
��None=�$	��Top=�$
��1015��$ ��Images/launcher_bg_rff.png��$��	10,0,0,-3q��� -   $��
imTextUser��$��33��$
��Left=�$��725,189,0,0q�$
��Fill=�$	��Top=�$	��262��$+��%/rlgn;component/Images/text_field.png���� -   $��
imTextPass��$��33��$
��Left=�$��726,228,0,0q�$
��Fill=�$	��Top=�$	��262��$+��%/rlgn;component/Images/text_field.png���� -   $��tbUser��$��31��$
��Left=�$��727,191,0,0q�$	��Top=�$	��261��$��Username�� �� 	MaxLength$ 24��	 �� MaxLines$	 1��$��1��
�������$��16(�
 J� Opacity$
 1Q���Z� .������ .��G� -   $��
tbPassword��$��31��$
��Left=�$��727,230,0,0q�$	��Top=�$	��261�� G� 	MaxLength$ 24��$��2��$��16(�$
 1Q�
������� G� Password$ Password����Z� ���� �� -   $��cbResolution��$��23��$��847,125,0,0q�$	��Top=�$y�Hidden=�$
��Left=�$	��120���� -   $��	cbTexture��$��23��$
��Left=�$��847,183,0,0q�$	��Top=�$	��120��$y�Hidden=��� -	   $��cbDLight��$��23��$
��Left=�$��847,212,0,0q�$	��Top=�$	��120��$y�Hidden=��� -
   $��cbGlowEffect��$��23��$
��Left=�$��847,241,0,0q�$	��Top=�$	��120��$y�Hidden=��� -   $��cbShadow��$��23��$
��Left=�$��847,270,0,0q�$	��Top=�$	��120��$y�Hidden=��� -   $��cbGamma��$��23��$
��Left=�$��847,299,0,0q�$	��Top=�$	��120��$y�Hidden=��� -   $��cbMouseAccel��$��Mouse Acceleration��$��16��$
��Left=�$��845,350,0,0q�$	��Top=� d� 	IsChecked$
 TrueY�$y�Hidden=�
��������� -   $��	cbDetText��$��Detailed Textures��$��16��$
��Left=�$��847,328,0,0q�$	��Top=�$
 TrueY�$y�Hidden=�
��������� -   $��label3��$��Fullcreen Resolution��$��28��$
��Left=�$��710,120,0,0q�$	��Top=�$y�Hidden=�
��������� -   $��label4��$��Texture Detail��$��28��$
��Left=�$��744,178,0,0q�$	��Top=�$y�Hidden=�
��������� -   $��label5��$��Dynamic Light��$��28��$
��Left=�$��741,207,0,0q�$	��Top=�$y�Hidden=�
��������� -   $��label6��$��Glow Effect��$��28��$
��Left=�$��757,236,0,0q�$	��Top=�$y�Hidden=�
��������� -   $��label7��$��Shadow��$��28��$
��Left=�$��775,265,0,0q�$	��Top=�$y�Hidden=�
��������� -   $��label8��$��Gamma��$��28��$
��Left=�$��776,294,0,0q�$	��Top=�$y�Hidden=�
��������� -   $��label9��$��Graphics Adapter��$��28��$
��Left=�$��
725,91,0,0q�$	��Top=�$y�Hidden=�
��������� -   $��	cbGraphAd��$��23��$
��Left=�$��
847,96,0,0q�$	��Top=�$y�Hidden=�$	��120���� -   $��imLoginNormal��$��40��$
��Left=�$��737,304,0,0q�$
��Fill=�$	��Top=�$	��106��$-��'/rlgn;component/Images/login_normal.png��$y�Visible=��� -   $��imLoginHover��$��40��$
��Left=�$��737,304,0,0q�$
��Fill=�$	��Top=�$	��106��$,��&/rlgn;component/Images/login_hover.png��$y�Visible=��� -   $��bLogin��$��Login��$��40��$
��Left=�$��735,304,0,0q�$	��Top=�$	��106�� �� 	IsDefault . $��4��$
 0Q��� -   $��btCancel��$��16��$
��Left=�$��
1008,5,0,0q�$	��Top=�$��13�� �� IsCancel . $
 0Q� J� RenderTransformOrigin$ 0.538,0.438&���Z� ���� ���� �� -   $��imSetupNormal��$��40��$
��Left=�$��874,304,0,0q�$
��Fill=�$	��Top=�$	��106��$/��)/rlgn;component/Images/options_normal.png���� -   $��imSetupHover��$��40��$
��Left=�$��874,304,0,0q�$
��Fill=�$	��Top=�$	��106��$.��(/rlgn;component/Images/options_hover.png���� -   $��btSetup��$��40��$
��Left=�$��874,304,0,0q�$	��Top=�$	��106��$
 0Q��� -   $��lbLoginDetails��$��Login Details��$��33��$
��Left=�$��727,159,0,0q�$	��Top=�$	��110��$��16(�
�������$y�Visible=��� -   $��cbWindowMode��$��Window Mode��$��28��$
��Left=�$��727,270,0,0q�$	��Top=�$	��200��
�������$��16(�$y�Visible=��� -    $��lbSetup��$��Client Setup��$��16(�
�������$��33��$
��Left=�$��
737,96,0,0q�$	��Top=�$	��110��$y�Hidden=��� -!   $��btSaveSetup��$
��Save��$��23��$
��Left=�$��774,412,0,0q�$	��Top=�$��75��$y�Hidden=��� -"   $��btCancelSetup��$��Cancel��$��23��$
��Left=�$��859,412,0,0q�$	��Top=�$��75��$y�Hidden=��� -#   $��	imSoundOn��$��16��$
��Left=�$��950,268,0,0q�$
��Fill=�$	��Top=�$��16��$(��"/rlgn;component/Images/speakon.png��$y�Hidden=��� -$   $��
imSoundOff��$��16��$
��Left=�$��950,268,0,0q�$
��Fill=�$	��Top=�$��16��$)��#/rlgn;component/Images/speakoff.png��$y�Hidden=��� -%   $��btVolume��$��Button��$��16��$
��Left=�$��950,267,0,0q�$	��Top=�$��16��$
 0Q�{�.  �� -&   $��	lbVersion��$��v1.314��$��28��$
��Left=�$��956,487,0,0q�$	��Top=�
�������$��58���� -'   $��lbSelectZone��$��Select Server:��$��28��$
��Left=�$��
16,194,0,0q�$	��Top=�$��16(�$
��Bold"�$y�Hidden=�
��������� -(   $��cbZones��$��28��$
��Left=�$��
16,231,0,0q�$	��Top=�$	��262��$��16(�$y�Hidden=�$
��Bold"� � 	Resources  �� Color��%C   (��       * SystemColors.HighlightBrushKey+)�� $ 	#FFCEACEB���� -)   $��lbPatchVersion��$��Patch Version: ��$��28��$
��Left=�$��
64,480,0,0q�$	��Top=�$	��138��
�������$y�Hidden=��� -*   $��cbWMResolution��$��23��$
��Left=�$��847,154,0,0q�$	��Top=�$y�Hidden=�$	��120���� -+   $��label1��$��Window Mode Resolution��
�������$��28��$
��Left=�$��679,149,0,0q�$	��Top=�$y�Hidden=�