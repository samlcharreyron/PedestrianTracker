   M S B A M L   `   `   `  ����   PedestrianTracker    PedestrianTracker.Options   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  Options��$	��300��$	��300�� � Icon$ icons/pedtrac.ico�� � 
ResizeMode$ NoResize=�.���  � RowDefinitions �� �� �� $
��Auto ���  � ColumnDefinitions �� $
��Auto ��� .+��� $
��Bold"�$��0��$��0��.��$"Horizontal angle of Kinect to road�� -   $��KinectAngleText��$��20��$��30��$	��Top=�$��0��$��1������    Settings �� Source# [   �� Path$ Default.Angle��� $
��Bold"�$��1��$��0��.��!Tracking data frames per sample�� $
��Bold"�$��20��$��30��$	��Top=�$��1��$��1������ # [  $! Default.TrajectorySubsample��� $��0��$��2��$
��Bold"� �� Target ��  �� ElementName$ ElevationAngle��.��ElevationAngle�� $��1��$��2��.+��� {�.  ���� $ ElevationAngle��$ Value��� -   $��ElevationAngle��	 �� Orientation$	 Vertical=�
 � Minimum$	
 -27Q� � Maximum$ 27Q�$	��100��$��Center=� �� TickFrequency$ 1Q� �� TickPlacement$ BottomRight=� �� IsSnapToTickEnabled .  � Value �� # [  $ Default.VerticalAngle��� $
��Bold"�$��0��$��3��.��'%Milliseconds between database updates�� $��1��$��3��$��20��$��60��$	��Top=����� # [  $ Default.dbTimerSetting�