   M S B A M L   `   `   `  ���� L  HPedestrianTracker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:PedestrianTrackerPedestrianTracker  "    PedestrianTracker.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -lclr-namespace:PedestrianTracker     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -     � Title$  Pedestrian Counter�� � Icon$ icons/pedtrac.ico��$	��670��$	��650��$	��670�� � 
ResizeMode$ CanResizeWithGrip=� � 	Resources +   %PedestrianTracker.VisibilityConverter   VisibilityConverter2  ,PedestrianTracker.RoadAxisY1PaddingConverter  RoadAxisY1PaddingConverter2  ,PedestrianTracker.RoadAxisY2PaddingConverter  RoadAxisY2PaddingConverter��%-   &	        &	      &	 
           .��� -   $��MainGrid�� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* ��� $��* �.+�]� .+�o� -   $��menu1��$��0��$��22��.@�m� $��_File��.@�m� -   $��_Open�� m� Icon �� $��icons\openHS.png��$��16��$��16��m� -   $��_Save As�� �� $��icons\saveHS.png��$��16��$��16���� m� -   $��E_xit��m� $��_View��.@�m� -   $��Show Tracked Data��m� -   $��Show Past Trajectories�� m� IsCheckable . m� -	   $��Show Road Axis�� . m� -
   $��Show 3D Trajectory View��m� $��_Tools��.@�m� -   $��Options��9� -    9�Name$ Expander�� 9� ExpandDirection$
 Down=�$��1��.���� -   	 ��Name$	 C1��.+��� -   $��image1������ 
 �� Path$
 RenderHeight����� $
 RenderHeight�'� -   $��viewport������  �� ElementName$ image1��$
 ActualHeight����� $ image1��$
 ActualWidth� '� Camera =� -   
camera�� � LookDirection$ 0,0,-15� � UpDirection$ 0,1,05� � Position$ 0,0,01� =� FieldOfView$ 45Q�.x�g� -   	model�� g� Content h� -   	group��.����  �� Color$ DarkGray��t� $ White�� t� 	Direction$ -5,-5,-75�"   PedestrianTracker.Trajectory  -     Name$ trajectoryCanvas1������ $ image1��$
 ActualWidth����� $ image1��$
 ActualHeight�  -   $ trajectoryCanvas2������ $ image1��$
 ActualWidth����� $ image1��$
 ActualHeight�  -   $ trajectoryCanvas3������ $ image1��$
 ActualWidth����� $ image1��$
 ActualHeight�  -   $ trajectoryCanvas4������ $ image1��$
 ActualWidth����� $ image1��$
 ActualHeight�  -   $ trajectoryCanvas5������ $ image1��$
 ActualWidth����� $ image1��$
 ActualHeight�  -   $ trajectoryCanvas6������ $ image1��$
 ActualWidth����� $ image1��$
 ActualHeight��� -    ��Name$ pastTrajectories������ $ image1��$
 ActualWidth����� $ image1��$
 ActualHeight�� -   $��MessageGrid��$��3��$��30��$��Bottom=� � ColumnDefinitions �� $��* ��� $��* ��� $��* �.+���  �� Orientation$ 
Horizontal=�$��0��.+��� -   $��TotalPlayersLabel��$��People in Frame: ��$��20(��� -   $��TotalPlayersDisplay��$��20(����� $
 TotalPlayers��� $ 
Horizontal=�$��1��.+��� $��   Pedestrian Counts: ��$��20(��� -   $��PedestrianCountsDisplay��$��20(����� $
 PedestrianCounts��� $ 
Horizontal=�$��2��.+��� $��   Tracking Angle: ��$��20(��� -   $��TrackingAngleDisplay��$��20(�����   Settings �� Source# [ $
 Default.KinectAngle��� -   $��errorBox��$��1��$��20(�
���  ��